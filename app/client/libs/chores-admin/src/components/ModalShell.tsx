//candidate for admin-ui
import React from "react";

type Props = Readonly<{
    open: boolean;
    busy: boolean;
    onRequestClose: () => void;
    widthStyle?: React.CSSProperties;
    children: React.ReactNode;
}>;

export function ModalShell(props: Props): React.ReactElement {
    const backdropClassName: string = `modal-backdrop ${props.open && "visible"}`;

    return (
        <div className={backdropClassName} aria-hidden={!props.open}>
            <div className="modal" style={props.widthStyle}>
                {props.children}
            </div>
        </div>
    );
}
