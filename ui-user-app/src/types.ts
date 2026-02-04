export interface FileResponse {
    id: string;
    fileName: string;
    fileSize: number;
    fileHash: string;
    backupUrl: string;
    remoteUrl: string;
    description: string;
    createTime: string;
    updateTime: string;
}