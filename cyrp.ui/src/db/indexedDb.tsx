import { openDB } from "idb";
import type { FileDbType } from "./types/file.dbType";

const INDEXEDB_DATABASE_NAME = "cypr.db";
const FILE_TABLE_OBJECT = "FILES";

export const StartDB = openDB(INDEXEDB_DATABASE_NAME, 2, {
    async upgrade(db, oldVersion, _newVersion, transaction) {
        if (oldVersion === 0) {
            db.createObjectStore(FILE_TABLE_OBJECT, {
                keyPath: "id",
                autoIncrement: true,
            });
        } else if (oldVersion < 2) {
            const existingFiles = await transaction.objectStore(FILE_TABLE_OBJECT).getAll() as Array<Partial<FileDbType>>;
            db.deleteObjectStore(FILE_TABLE_OBJECT);
            const fileStore = db.createObjectStore(FILE_TABLE_OBJECT, {
                keyPath: "id",
                autoIncrement: true,
            });

            existingFiles.forEach((file) => {
                fileStore.add({
                    name: file.name ?? "",
                    size: Number(file.size) || 0,
                    lastModified: file.lastModified ?? 0,
                    type: file.type ?? "",
                });
            });
        }
    }
});

export async function GetFile(fileId: number): Promise<FileDbType> {
    const db = await StartDB;
    const fileDb = await db.get(FILE_TABLE_OBJECT, fileId) as FileDbType | undefined;

    var file: FileDbType = {
        name: '',
        size: 0,
        lastModified: 0,
        type: '',
    };

    if (fileDb != undefined) {
        file = {
            name: fileDb.name,
            size: fileDb.size,
            lastModified: fileDb.lastModified,
            type: fileDb.type,
        }
    }

    return file;
}

export async function GetAll(): Promise<FileDbType[]> {
    try {
        const db = await StartDB;
        const files = await db.getAll(FILE_TABLE_OBJECT) as FileDbType[];
        return files;
    }
    catch (error) {
        console.error("Error retrieving files from IndexedDB:", error);
        return Promise.reject(error);
    }
}

export async function AddFile(file: File) {
    try {
        const db = await StartDB;
        const fileDb: FileDbType = {
            name: file.name,
            size: file.size,
            lastModified: file.lastModified,
            type: file.type,
        };
        await db.put(FILE_TABLE_OBJECT, fileDb);
    } catch (error) {
        console.error("Error adding file to IndexedDB:", error);
        return Promise.reject(error);
    }
}

export async function RemoveFile(fileId: number) {
    try {
        const db = await StartDB;
        await db.delete(FILE_TABLE_OBJECT, fileId);
    } catch (error) {
        console.error("Error removing file to IndexedDB:", error);
        return Promise.reject(error);
    }
}

export async function ClearBrowserData() {
    try {
        const db = await StartDB;
        db.clear(FILE_TABLE_OBJECT);
    } catch (e) {
        console.error("Erro clearing browser data:", e);
    }
}

export async function SavedFilesUi() {
    try {
        const db = await StartDB;
        const files = await db.getAll(FILE_TABLE_OBJECT) as FileDbType[];

        return files.length;
    } catch (e) {
        console.error("Error trying to return saved file info to UI", e);
    }
}