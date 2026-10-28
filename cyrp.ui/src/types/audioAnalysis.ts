export type AudioType = "meeting" | "class" | "unrelated";
export type ActionColumn = "backlog" | "todo" | "doing" | "done";
export type ActionPriority = "low" | "medium" | "high";

export type TimeNote = {
    timestamp: string | null;
    topic: string;
    note: string;
};

export type ActionItem = {
    title: string;
    description: string;
    column: ActionColumn;
    priority: ActionPriority;
    owner: string | null;
    dueDate: string | null;
    timestamp: string | null;
};

export type GeneratedFile = {
    fileName: string;
    type: "mermaid";
    description: string;
    content: string;
};

export type AudioAnalysis = {
    audioType: AudioType;
    summary: string;
    timeNotes: TimeNote[];
    actions: ActionItem[];
    studyNotes: string[];
    filesToGenerate: GeneratedFile[];
};
