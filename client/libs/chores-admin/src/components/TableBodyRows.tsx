//candidate for admin-ui
type EmptyStateProps<TItem> = {
    loading: boolean;
    items: readonly TItem[];
    search: string;
    colSpan: number;
    loadingText: string;
    emptyText: string;
    emptyWhenSearchingText: string;
    renderRows: (items: readonly TItem[]) => React.ReactNode;
};

export function TableBodyRows<TItem>(props: EmptyStateProps<TItem>): React.ReactElement {
    const trimmed: string = props.search.trim();

    if (props.loading) {
        return (
            <tr>
                <td colSpan={props.colSpan} className="empty">
                    {props.loadingText}
                </td>
            </tr>
        );
    }

    if (props.items.length === 0) {
        const text: string = trimmed.length > 0 ? props.emptyWhenSearchingText : props.emptyText;

        return (
            <tr>
                <td colSpan={props.colSpan} className="empty">
                    {text}
                </td>
            </tr>
        );
    }

    return <>{props.renderRows(props.items)}</>;
}
