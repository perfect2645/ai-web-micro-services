interface ImportMetaEnv {
  readonly VITE_SIGNALR_HUB_URL: string;
  readonly VITE_FILE_UPLOAD_URL: string;
  readonly VITE_DORAEMON_API_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
