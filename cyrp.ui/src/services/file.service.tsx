import { FFmpeg, type FileData } from '@ffmpeg/ffmpeg';
import { fetchFile } from '@ffmpeg/util';
import axios, { HttpStatusCode } from 'axios';

// funnyNotes: been discussing 30 min with AI about implementation of file conversions in back or frontend 
// and its implications, decided to implement video->audio conversion in front so the request in the HTTP layer goes fast
const BACKEND_BASE_ADDRESS = 'http://localhost:5175/';

export async function ConvertVideoFileToAudio(file: File): Promise<FileData> {
    try {
        const ffmpeg = new FFmpeg();
        await ffmpeg.load();
        await ffmpeg.writeFile('in', await fetchFile(file));
        await ffmpeg.exec(['-i', 'in', '-vn', '-ac', '1', '-ar', '16000', '-b:a', '32k', 'out.mp3']);
        const data = await ffmpeg.readFile('out.mp3');
        return data;
    }
    catch (e) {
        throw e;
    }
}

export async function SendFilesToBackend(files: File[] | FileData[]): Promise<void> {
    try {
        var req = await axios.post("uploadFiles", files, {
            baseURL: BACKEND_BASE_ADDRESS,
            maxContentLength: 25 * 1024 * 1024 // 26214400 MB = 25.600 GB (in binary)
        }).then((response) => {
            if (response.status == 
                HttpStatusCode.InternalServerError 
                || HttpStatusCode.BadGateway 
                || HttpStatusCode.BadRequest)
                Promise.reject("Error occourred trying to reach backend, check logs.");
        });
    }
    catch (e) {
        throw e;
    }
}