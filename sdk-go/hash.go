package pan123

import (
	"crypto/md5"
	"encoding/hex"
	"io"
	"os"
)

func md5File(path string) (string, error) {
	file, err := os.Open(path)
	if err != nil {
		return "", err
	}
	defer file.Close()

	hash := md5.New()
	if _, err := io.Copy(hash, file); err != nil {
		return "", err
	}
	return hex.EncodeToString(hash.Sum(nil)), nil
}

func md5Bytes(data []byte) string {
	sum := md5.Sum(data)
	return hex.EncodeToString(sum[:])
}
