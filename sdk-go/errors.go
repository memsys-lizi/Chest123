package pan123

import (
	"fmt"
	"net/http"
)

// APIError represents an HTTP or 123Pan business error.
type APIError struct {
	Code         int
	Message      string
	TraceID      string
	StatusCode   int
	ResponseBody string
}

func (e *APIError) Error() string {
	if e == nil {
		return "<nil>"
	}
	if e.Code != 0 {
		return fmt.Sprintf("pan123: code %d: %s", e.Code, e.Message)
	}
	if e.StatusCode != 0 {
		return fmt.Sprintf("pan123: http %d: %s", e.StatusCode, e.Message)
	}
	return "pan123: " + e.Message
}

func isRateLimitStatus(status int) bool {
	return status == http.StatusTooManyRequests
}
