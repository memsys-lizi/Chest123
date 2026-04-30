package pan123

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"mime/multipart"
	"net/http"
	"net/url"
	"reflect"
	"strconv"
	"strings"
)

func (c *Client) send(ctx context.Context, method, path string, opts RequestOptions, out any) error {
	headers := make(http.Header)
	headers.Set("Platform", c.platform)
	for key, value := range opts.Headers {
		headers.Set(key, value)
	}

	if !opts.NoAuth {
		token, err := c.ensureAccessToken(ctx)
		if err != nil {
			return err
		}
		headers.Set("Authorization", "Bearer "+token)
	}

	body, contentType, err := buildRequestBody(opts)
	if err != nil {
		return err
	}
	if contentType != "" {
		headers.Set("Content-Type", contentType)
	}

	requestURL, err := buildURL(firstNonEmpty(opts.BaseURL, c.baseURL), path, opts.Query)
	if err != nil {
		return err
	}
	req, err := http.NewRequestWithContext(ctx, method, requestURL, body)
	if err != nil {
		return err
	}
	req.Header = headers

	resp, err := c.httpClient.Do(req)
	if err != nil {
		return err
	}
	defer resp.Body.Close()

	responseBody, err := io.ReadAll(resp.Body)
	if err != nil {
		return err
	}
	if resp.StatusCode < 200 || resp.StatusCode >= 300 {
		return &APIError{
			Message:      fmt.Sprintf("HTTP request failed with status %d.", resp.StatusCode),
			StatusCode:   resp.StatusCode,
			ResponseBody: string(responseBody),
		}
	}
	if len(bytes.TrimSpace(responseBody)) == 0 {
		return nil
	}

	var wrapped pan123Response
	if json.Unmarshal(responseBody, &wrapped) == nil && looksWrapped(responseBody) {
		if wrapped.Code != 0 {
			return &APIError{
				Code:         wrapped.Code,
				Message:      wrapped.Message,
				TraceID:      wrapped.TraceID,
				StatusCode:   resp.StatusCode,
				ResponseBody: string(responseBody),
			}
		}
		return decodeData(wrapped.Data, out)
	}
	return decodeData(responseBody, out)
}

func buildRequestBody(opts RequestOptions) (io.Reader, string, error) {
	if opts.Form != nil {
		pr, pw := io.Pipe()
		writer := multipart.NewWriter(pw)
		go func() {
			err := writeMultipart(writer, opts.Form)
			closeErr := writer.Close()
			if err == nil {
				err = closeErr
			}
			_ = pw.CloseWithError(err)
		}()
		return pr, writer.FormDataContentType(), nil
	}
	if opts.Body == nil {
		return nil, "", nil
	}
	data, err := json.Marshal(opts.Body)
	if err != nil {
		return nil, "", err
	}
	return bytes.NewReader(data), "application/json", nil
}

func writeMultipart(writer *multipart.Writer, form *MultipartForm) error {
	for key, value := range form.Fields {
		if isNilAny(value) {
			continue
		}
		if err := writer.WriteField(key, formValueToString(value)); err != nil {
			return err
		}
	}
	for _, file := range form.Files {
		part, err := writer.CreateFormFile(file.FieldName, file.FileName)
		if err != nil {
			return err
		}
		if _, err := io.Copy(part, file.Reader); err != nil {
			return err
		}
	}
	return nil
}

func isNilAny(value any) bool {
	if value == nil {
		return true
	}
	rv := reflect.ValueOf(value)
	switch rv.Kind() {
	case reflect.Chan, reflect.Func, reflect.Interface, reflect.Map, reflect.Pointer, reflect.Slice:
		return rv.IsNil()
	default:
		return false
	}
}

func formValueToString(value any) string {
	rv := reflect.ValueOf(value)
	for rv.IsValid() && (rv.Kind() == reflect.Pointer || rv.Kind() == reflect.Interface) {
		if rv.IsNil() {
			return ""
		}
		rv = rv.Elem()
	}
	if rv.IsValid() {
		value = rv.Interface()
	}
	switch v := value.(type) {
	case string:
		return v
	case bool:
		return strconv.FormatBool(v)
	case int:
		return strconv.Itoa(v)
	case int64:
		return strconv.FormatInt(v, 10)
	case float64:
		return strconv.FormatFloat(v, 'f', -1, 64)
	default:
		return fmt.Sprint(v)
	}
}

func buildURL(baseURL, path string, query any) (string, error) {
	root := strings.TrimRight(baseURL, "/")
	relative := path
	if !strings.HasPrefix(relative, "/") {
		relative = "/" + relative
	}
	values, err := queryValues(query)
	if err != nil {
		return "", err
	}
	if len(values) == 0 {
		return root + relative, nil
	}
	return root + relative + "?" + values.Encode(), nil
}

func queryValues(query any) (url.Values, error) {
	values := url.Values{}
	if query == nil {
		return values, nil
	}
	rv := reflect.ValueOf(query)
	if rv.Kind() == reflect.Pointer {
		if rv.IsNil() {
			return values, nil
		}
		rv = rv.Elem()
	}
	switch rv.Kind() {
	case reflect.Map:
		for _, key := range rv.MapKeys() {
			if key.Kind() != reflect.String {
				continue
			}
			addQueryValue(values, key.String(), rv.MapIndex(key))
		}
	case reflect.Struct:
		rt := rv.Type()
		for i := 0; i < rv.NumField(); i++ {
			field := rt.Field(i)
			if field.PkgPath != "" {
				continue
			}
			name, omitEmpty := fieldName(field)
			if name == "" || name == "-" {
				continue
			}
			value := rv.Field(i)
			if omitEmpty && isEmptyValue(value) {
				continue
			}
			addQueryValue(values, name, value)
		}
	default:
		return values, nil
	}
	return values, nil
}

func fieldName(field reflect.StructField) (string, bool) {
	tag := field.Tag.Get("url")
	if tag == "" {
		tag = field.Tag.Get("json")
	}
	if tag == "" {
		return field.Name, false
	}
	parts := strings.Split(tag, ",")
	omit := false
	for _, part := range parts[1:] {
		if part == "omitempty" {
			omit = true
		}
	}
	return parts[0], omit
}

func addQueryValue(values url.Values, key string, value reflect.Value) {
	if !value.IsValid() {
		return
	}
	if value.Kind() == reflect.Interface {
		if value.IsNil() {
			return
		}
		value = value.Elem()
	}
	if value.Kind() == reflect.Pointer {
		if value.IsNil() {
			return
		}
		value = value.Elem()
	}
	values.Set(key, fmt.Sprint(value.Interface()))
}

func isEmptyValue(value reflect.Value) bool {
	if !value.IsValid() {
		return true
	}
	switch value.Kind() {
	case reflect.Pointer, reflect.Interface, reflect.Slice, reflect.Map:
		return value.IsNil()
	case reflect.String:
		return value.Len() == 0
	case reflect.Bool:
		return !value.Bool()
	case reflect.Int, reflect.Int8, reflect.Int16, reflect.Int32, reflect.Int64:
		return value.Int() == 0
	default:
		return false
	}
}

func looksWrapped(data []byte) bool {
	var probe map[string]json.RawMessage
	if err := json.Unmarshal(data, &probe); err != nil {
		return false
	}
	_, hasCode := probe["code"]
	_, hasMessage := probe["message"]
	return hasCode && hasMessage
}

func decodeData(data []byte, out any) error {
	if out == nil || len(bytes.TrimSpace(data)) == 0 || bytes.Equal(bytes.TrimSpace(data), []byte("null")) {
		return nil
	}
	if raw, ok := out.(*json.RawMessage); ok {
		*raw = append((*raw)[:0], data...)
		return nil
	}
	return json.Unmarshal(data, out)
}

func firstNonEmpty(values ...string) string {
	for _, value := range values {
		if value != "" {
			return value
		}
	}
	return ""
}
