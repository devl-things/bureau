// Enums matching server contracts (Watson.Nodes.Contracts.Dtos)

export enum NodeKind {
    Item = 1,
    Tag = 2,
    Variant = 3,
    Project = 4,
}

export enum AttributeValueType {
    String = 1,
    Number = 2,
    Bool = 3,
    Json = 4,
    Ref = 5,
    Date = 6,
}

export enum AttributeCardinality {
    Single = 1,
    ManyUnordered = 2,
    ManyOrdered = 3,
}

export enum NodeStatus {
    Active = 1,
    Archived = 2,
}

// DTOs

export type AttributeDto = {
    key: string;
    locale?: string;
    type: AttributeValueType;
    valueString?: string | null;
    valueNumber?: number | null;
    valueBool?: boolean | null;
    valueJson?: string | null;
    refNodeId?: string | null;
    valueDate?: string | null;
    cardinality: AttributeCardinality;
    position?: number | null;
};

export type NodeDto = {
    nodeId: string;
    kind: NodeKind;
    scope: string;
    canonicalKey: string;
    status: NodeStatus;
    version: number;
    attributes: AttributeDto[];
};

export type CreateNodeRequest = {
    canonicalKey?: string | null;
    scope?: string | null;
    attributes?: AttributeDto[] | null;
};

export type PatchNodeAttributesRequest = {
    set?: AttributeDto[] | null;
    remove?: { key: string; locale?: string }[] | null;
};

export type SearchNodesQuery = {
    query?: string;
    scope?: string;
    locale?: string;
    cursor?: string;
    limit?: number;
};

export type EdgeDto = {
    targetNodeId: string;
    purpose: string;
    orderIndex: number;
};

export type CreateEdgeRequest = {
    targetNodeId: string;
    purpose: string;
    orderIndex: number;
};

export type RemoveEdgeRequest = {
    targetNodeId: string;
    purpose: string;
};

// Cursor pagination (matches BureauCursorResponse)

export type CursorMeta = {
    cursor?: string | null;
    nextCursor: string;
    hasMore: boolean;
    count: number;
};

export type CursorResponse<T> = {
    data: T[];
    meta: CursorMeta;
};
