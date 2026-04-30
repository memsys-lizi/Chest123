package pan123

import (
	"context"
	"strings"
	"time"
)

func retryWhileFileChecking[T any](ctx context.Context, attempts int, delay time.Duration, task func() (T, error)) (T, error) {
	attempts = positive(attempts, 60)
	delay = defaultDuration(delay, time.Second)
	var zero T
	for i := 1; i <= attempts; i++ {
		value, err := task()
		if err == nil {
			return value, nil
		}
		if !isFileCheckingError(err) || i == attempts {
			return zero, err
		}
		if err := sleepContext(ctx, delay); err != nil {
			return zero, err
		}
	}
	return zero, &APIError{Code: fileCheckingCode, Message: "File checking did not finish before retry attempts were exhausted."}
}

func retryTransientUpload[T any](ctx context.Context, attempts int, delay time.Duration, task func() (T, error)) (T, error) {
	attempts = positive(attempts, 5)
	delay = defaultDuration(delay, time.Second)
	var zero T
	for i := 1; i <= attempts; i++ {
		value, err := task()
		if err == nil {
			return value, nil
		}
		if !isTransientUploadError(err) || i == attempts {
			return zero, err
		}
		if err := sleepContext(ctx, delay); err != nil {
			return zero, err
		}
	}
	return zero, &APIError{Message: "Transient upload retry attempts were exhausted."}
}

func isFileCheckingError(err error) bool {
	apiErr, ok := err.(*APIError)
	return ok && apiErr.Code == fileCheckingCode
}

func isTransientUploadError(err error) bool {
	apiErr, ok := err.(*APIError)
	if !ok {
		return false
	}
	if apiErr.Code == 429 || isRateLimitStatus(apiErr.StatusCode) {
		return true
	}
	return apiErr.Code == 1 &&
		(strings.Contains(apiErr.Message, "秒传队列") ||
			strings.Contains(apiErr.Message, "削峰") ||
			strings.Contains(apiErr.Message, "请慢一点"))
}

func sleepContext(ctx context.Context, delay time.Duration) error {
	if delay <= 0 {
		return nil
	}
	timer := time.NewTimer(delay)
	defer timer.Stop()
	select {
	case <-ctx.Done():
		return ctx.Err()
	case <-timer.C:
		return nil
	}
}

func positive(value, fallback int) int {
	if value <= 0 {
		return fallback
	}
	return value
}

func defaultDuration(value, fallback time.Duration) time.Duration {
	if value <= 0 {
		return fallback
	}
	return value
}
