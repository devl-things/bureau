import * as SQLite from "expo-sqlite";
import { ExpoSqliteStorage } from "./ExpoSqliteStorage";

let _instance: ExpoSqliteStorage | null = null;

export async function getStorage(): Promise<ExpoSqliteStorage> {
    if (_instance) return _instance;

    const db = await SQLite.openDatabaseAsync("bureau-offline.db");
    _instance = new ExpoSqliteStorage(db);
    await _instance.initialize();
    return _instance;
}
