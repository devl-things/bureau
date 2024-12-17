import React, { useState, useEffect } from "react";

function TagModal({ onClose, onSave, value }) {
  const [tag, setTag] = useState("");

  useEffect(() => {
    setTag(value); // Set the initial value when the modal opens
  }, [value]);

  const handleSave = () => {
    onSave(tag);
    setTag("");
  };

  return (
    <div className="modal d-block" tabIndex={-1}>
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">Enter Tag</h5>
            <button type="button" className="btn-close" onClick={onClose}></button>
          </div>
          <div className="modal-body">
            <input
              type="text"
              className="form-control"
              value={tag}
              onChange={(e) => setTag(e.target.value)}
              placeholder="Enter tag"
            />
          </div>
          <div className="modal-footer">
            <button className="btn btn-secondary" onClick={onClose}>
              Cancel
            </button>
            <button className="btn btn-primary" onClick={handleSave}>
              Save
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

export default TagModal;
