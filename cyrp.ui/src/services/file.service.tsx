import { FFmpeg } from '@ffmpeg/ffmpeg';
import { fetchFile } from '@ffmpeg/util';
import axios from 'axios';
import type { AudioAnalysis } from '../types/audioAnalysis';

const BACKEND_BASE_ADDRESS = import.meta.env.BACKEND_BASE_ADDRESS;

export async function ConvertVideoFileToAudio(file: File): Promise<File> {
    const ffmpeg = new FFmpeg();
    await ffmpeg.load();
    await ffmpeg.writeFile('in', await fetchFile(file));
    await ffmpeg.exec(['-i', 'in', '-vn', '-ac', '1', '-ar', '16000', '-b:a', '32k', 'out.mp3']);
    const data = await ffmpeg.readFile('out.mp3');
    if (typeof data === 'string') {
        throw new Error('FFmpeg returned text instead of audio data.');
    }

    const audioName = file.name.replace(/\.[^.]+$/, '') + '.mp3';
    return new File([new Uint8Array(data)], audioName, { type: 'audio/mpeg' });
}

export async function SendFilesToBackend(files: File[]): Promise<AudioAnalysis> {
    const formData = new FormData();
    files.forEach((file) => formData.append('files', file, file.name));

    const response = await axios.post<AudioAnalysis>('files/uploadfile', formData, {
        baseURL: BACKEND_BASE_ADDRESS,
        maxBodyLength: 25 * 1024 * 1024,
        maxContentLength: 25 * 1024 * 1024
    });

    return response.data;
}
