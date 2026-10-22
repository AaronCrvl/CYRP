import { useEffect, useState } from "react"
import { allowedMimeTypes } from "../types/fileTypes";
import type { UploadTypes } from "../types/uploadTypes";

type Toast = {
    type: "success" | "error";
    message: string;
};

export default function FileUploader() {
    const [files, setFiles] = useState<File[] | null>(null)
    const [uploadStatus, setUploadStatus] = useState<UploadTypes>('no file');
    const [fileError, setFileError] = useState<string | null>(null);
    const [toast, setToast] = useState<Toast | null>(null);

    useEffect(() => {
        if (!toast) return;

        const timeoutId = window.setTimeout(() => setToast(null), 4000);
        return () => window.clearTimeout(timeoutId);
    }, [toast]);

    const handleFileInput = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const selectedFiles = Array.from(e.target.files);
            if (selectedFiles.length === 0) return;

            const validFiles = selectedFiles.filter((file) =>
                allowedMimeTypes.some((type) => type === file.type)
            );

            setFiles(validFiles);
            const hasInvalidFiles = validFiles.length !== selectedFiles.length;
            const errorMessage = hasInvalidFiles
                ? "One or more files have an unsupported file type."
                : null;
            setFileError(errorMessage);
            setUploadStatus(hasInvalidFiles ? "error" : "ready");
            setToast(errorMessage
                ? { type: "error", message: errorMessage }
                : { type: "success", message: "Files added and ready to upload." });
        }
    }

    const handleFileUpload = () => {
        if (!files?.length) {
            const message = "Select at least one supported file before submitting.";
            setUploadStatus("error");
            setFileError(message);
            setToast({ type: "error", message });
            return;
        }

        setUploadStatus("ready");
        setToast({ type: "success", message: "Files are ready to upload." });
    }

    const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
        try {
            e.preventDefault();

            const droppedFiles = Array.from(e.dataTransfer.files);
            if (droppedFiles.length === 0) return;

            const validFiles = droppedFiles.filter((file) =>
                allowedMimeTypes.some((type) => type === file.type)
            );

            if (validFiles.length > 0) {
                setFiles((currentFiles) => [...(currentFiles ?? []), ...validFiles]);
            }
            const hasInvalidFiles = validFiles.length !== droppedFiles.length;
            const errorMessage = hasInvalidFiles
                ? "One or more files have an unsupported file type."
                : null;
            setFileError(errorMessage);
            setUploadStatus(hasInvalidFiles ? "error" : "ready");
            setToast(errorMessage
                ? { type: "error", message: errorMessage }
                : { type: "success", message: "Files added and ready to upload." });
            e.dataTransfer.clearData();
        }
        catch (error) {
            e.dataTransfer.clearData();
            setUploadStatus("error");
            const message = "Unable to add the dropped files.";
            setFileError(message);
            setToast({ type: "error", message });
            console.error(message, error);
        }
    }

    const handleFileDelete = (key: number) => {
        if (files != null) {
            var toDeleteItem = files[key];
            var newFilesArray: File[] = files.filter(file => file != toDeleteItem);
            setFiles(newFilesArray);
        }
    }

    const handleEdit = (_key: number) => {
        // rename and others...
    }

    return (
        <div className="file-uploader w-full max-w-2xl rounded-2xl border border-slate-200 bg-white p-6 text-left shadow-sm sm:p-8">
            {toast && (
                <div className="fixed right-4 top-4 z-50 w-[calc(100%-2rem)] max-w-sm">
                    <div
                        className={`flex items-start justify-between gap-4 rounded-xl border px-4 py-3 text-sm shadow-lg ${
                            toast.type === "success"
                                ? "border-emerald-200 bg-emerald-50 text-emerald-800"
                                : "border-red-200 bg-red-50 text-red-800"
                        }`}
                        role={toast.type === "error" ? "alert" : "status"}
                        aria-live={toast.type === "error" ? "assertive" : "polite"}
                    >
                        <p>{toast.message}</p>
                        <button
                            className="shrink-0 rounded-md px-2 font-semibold opacity-70 transition hover:opacity-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-current"
                            type="button"
                            aria-label="Dismiss notification"
                            onClick={() => setToast(null)}
                        >
                            ×
                        </button>
                    </div>
                </div>
            )}
            <div className="mb-6">
                <h1 className="mb-2 text-2xl font-semibold tracking-tight text-slate-900 sm:text-3xl">Upload a file</h1>
                <p className="text-sm text-slate-500">Choose a file or drag it into the drop zone below.</p>
            </div>

            <div
                className="rounded-xl border-2 border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center transition-colors hover:border-indigo-400 hover:bg-indigo-50/50"
                onDragOver={(e) => e.preventDefault()}
                onDrop={(e) => handleDrop(e)}
            >
                <div className="mx-auto mb-4 flex size-12 items-center justify-center rounded-full bg-indigo-100 text-indigo-600" aria-hidden="true">
                    <svg viewBox="0 0 24 24" fill="none" className="size-6" stroke="currentColor" strokeWidth="1.8">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 16V4m0 0L7 9m5-5 5 5M5 14v4a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2v-4" />
                    </svg>
                </div>
                <p className="mb-1 text-sm font-medium text-slate-700">Drop your file here</p>
                <p className="mb-5 text-xs text-slate-500">or browse from your device</p>
                <input
                    className="sr-only"
                    type="file"
                    id="file-input"
                    accept={allowedMimeTypes.join(",")}
                    multiple
                    onChange={handleFileInput}
                />
                <label
                    className="inline-flex cursor-pointer items-center rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition-colors hover:bg-indigo-700 focus-within:outline-2 focus-within:outline-offset-2 focus-within:outline-indigo-600"
                    htmlFor="file-input"
                >
                    Choose a file
                </label>
            </div>
            {fileError && <p className="mt-3 text-sm text-red-600" role="alert">{fileError}</p>}

            {files != null && (
                files.map((file, cont) =>
                    <div key={cont} className="mt-6 rounded-xl border border-slate-200 bg-white p-5">
                        <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
                            <h2 className="text-base font-semibold text-slate-900">File info</h2>
                            <span className="rounded-full bg-indigo-50 px-3 py-1 text-xs font-medium capitalize text-indigo-700">
                                {uploadStatus}
                            </span>
                            <button
                                className="rounded-lg border border-indigo-200 bg-white px-3 py-1.5 text-sm font-medium text-indigo-700 shadow-sm transition hover:border-indigo-300 hover:bg-indigo-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                                type="button"
                                onClick={() => handleEdit(cont)}
                            >
                                Edit
                            </button>
                            <button
                                className="rounded-lg border border-red-200 bg-white px-3 py-1.5 text-sm font-medium text-red-700 shadow-sm transition hover:border-red-300 hover:bg-red-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-red-600"
                                type="button"
                                onClick={() => handleFileDelete(cont)}
                            >
                                Delete
                            </button>
                        </div>
                        <dl className="grid gap-4 text-sm sm:grid-cols-2">
                            <div className="min-w-0">
                                <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Name</dt>
                                <dd className="mt-1 break-all font-medium text-slate-800">{file.name}</dd>
                            </div>
                            <div>
                                <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Size</dt>
                                <dd className="mt-1 text-slate-800">{file.size}</dd>
                            </div>
                            <div>
                                <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Last modified</dt>
                                <dd className="mt-1 text-slate-800">{file.lastModified}</dd>
                            </div>
                            <div className="min-w-0">
                                <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Type</dt>
                                <dd className="mt-1 break-all text-slate-800">{file.type}</dd>
                            </div>
                        </dl>
                    </div>
                ))}

            <div className="mt-6 flex justify-end">
                <button
                    className="rounded-lg bg-slate-900 px-5 py-2.5 text-sm font-medium text-white shadow-sm transition-colors hover:bg-slate-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-slate-900"
                    onClick={handleFileUpload}
                >
                    Submit
                </button>
            </div>
        </div>
    )
}