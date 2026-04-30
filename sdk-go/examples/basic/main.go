package main

import (
	"context"
	"fmt"
	"os"

	pan123 "github.com/memsys-lizi/Chest123/sdk-go"
)

func main() {
	client := pan123.NewClient(pan123.Options{
		ClientID:     os.Getenv("PAN123_CLIENT_ID"),
		ClientSecret: os.Getenv("PAN123_CLIENT_SECRET"),
	})

	files, err := client.Files.List(context.Background(), pan123.FileListRequest{
		ParentFileID: 0,
		Limit:        100,
	})
	if err != nil {
		panic(err)
	}

	fmt.Printf("files: %d\n", len(files.FileList))
}
