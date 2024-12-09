window.indexedDbManager = {
    db: null,

    openDb: function (dbName, storeName) {
        return new Promise((resolve, reject) => {
            if (this.db) {
                resolve(this.db);
                return;
            }

            const request = indexedDB.open(dbName, 1);
            request.onerror = (event) => reject(event);

            request.onsuccess = (event) => {
                this.db = event.target.result;
                resolve(this.db);
            };

            request.onupgradeneeded = (event) => {
                this.db = event.target.result;
                if (!this.db.objectStoreNames.contains(storeName)) {
                    this.db.createObjectStore(storeName, { keyPath: "id", autoIncrement: true });
                }
            };
        });
    },

    addData: function (dbName, storeName, data) {
        return this.openDb(dbName, storeName).then((db) => {
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([storeName], "readwrite");
                const store = transaction.objectStore(storeName);
                const request = store.add(data);

                request.onsuccess = () => resolve(request.result);
                request.onerror = (event) => reject(event);
            });
        });
    },

    getData: function (dbName, storeName, id) {
        return this.openDb(dbName, storeName).then((db) => {
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([storeName], "readonly");
                const store = transaction.objectStore(storeName);
                const request = store.get(id);

                request.onsuccess = () => resolve(request.result);
                request.onerror = (event) => reject(event);
            });
        });
    },

    getAllData: function (dbName, storeName) {
        return this.openDb(dbName, storeName).then((db) => {
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([storeName], "readonly");
                const store = transaction.objectStore(storeName);
                const request = store.getAll();

                request.onsuccess = () => resolve(request.result);
                request.onerror = (event) => reject(event);
            });
        });
    },

    deleteData: function (dbName, storeName, id) {
        return this.openDb(dbName, storeName).then((db) => {
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([storeName], "readwrite");
                const store = transaction.objectStore(storeName);
                const request = store.delete(id);

                request.onsuccess = () => resolve();
                request.onerror = (event) => reject(event);
            });
        });
    }
};
