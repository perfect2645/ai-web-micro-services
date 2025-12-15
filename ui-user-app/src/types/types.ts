export interface User {
  username: string;
  token: string;
}

export interface RecognitionResult {
  id: string;
  imageUrl: string;
  result: string;
  timestamp: number;
  status: 'pending' | 'success' | 'failed';
}

export interface Settings {
  maxUploadSize: number; // MB
  allowedFormats: string[];
  recognitionTimeout: number; // 秒
  resultSaveTime: number; // 天
  savePath: string;
  saveFormat: string;
  saveNameRule: string;
  maxSaveSize: number; // MB
  maxSaveCount: number;
}

export interface AppState {
  user: User | null;
  recognitionHistory: RecognitionResult[];
  settings: Settings;
}