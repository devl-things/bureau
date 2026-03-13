export type PagedMeta = {
    page: number;
    pageSize: number;
    total: number;
    totalPages: number;
    hasNext: boolean;
    hasPrevious: boolean;
};

export type PagedResponse<T> = {
    data: T[];
    meta: PagedMeta;
};
