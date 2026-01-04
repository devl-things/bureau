import type { ProblemDetails } from "./ProblemDetails";

export type ApiErrorRequestInfo = {
    method: string;
    url: string;
};

export class ApiError extends Error {
    public readonly status: number;
    public readonly request: ApiErrorRequestInfo;
    public readonly problem?: ProblemDetails;
    public readonly bodyText?: string;

    public constructor(message: string, status: number, request: ApiErrorRequestInfo, problem?: ProblemDetails, bodyText?: string) {
        super(message);
        this.name = "ApiError";
        this.status = status;
        this.request = request;
        this.problem = problem;
        this.bodyText = bodyText;
    }
}
