import { useState } from "react"

type uploadType = 'no file' | 'ready' | 'uploading' | 'completed' | 'error';

export default function FileUploader() {
    const [file, setFile] = useState<File | null>(null)
    const [uploadStatus, setUploadStatus] = useState<uploadType>('no file');

    const handleFileInput = (e: React.ChangeEvent<HTMLInputElement>) => {        
        if (e.target.files) {
            setFile(e.target.files[0]);            
            setUploadStatus("ready");
        }        
    }
    const handleFileUpload = () => {
        setUploadStatus('uploading');

        try {
            // base code
        }
        catch (e) {
            setUploadStatus("error");
        }
        finally {
            setFile(null);
            setUploadStatus("completed");
        }
    }

    return (
        <div className="file-uploader">
            <input type="file" id="file-input" onChange={handleFileInput} />
            <label htmlFor="file-input">Choose a file</label>


            {file &&
                <>
                    <label>File Info</label>
                    <label>Upload Status: {uploadStatus}</label>
                    <div>
                        <label>Name: {file.name}</label>
                        <label>Size: {file.size}</label>
                        <label>Last Modified: {file.lastModified}</label>
                        <label>Type: {file.type}</label>
                    </div>
                </>
            }

            <button onClick={handleFileUpload}>Submit</button>
        </div>
    )
}