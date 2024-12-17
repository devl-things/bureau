import React from 'react';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';

const DateModal = ({ show, onClose, onSave, value, setValue }) => {
  if (!show) return null;

  return (
    <div className="modal fade show d-block" tabIndex="-1" role="dialog">
      <div className="modal-dialog" role="document">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">Insert Date</h5>
            <button type="button" className="btn-close" onClick={onClose}></button>
          </div>
          <div className="modal-body">
            <DatePicker
              selected={value}
              onChange={(date) => setValue(date)}
              inline
            />
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-secondary" onClick={onClose}>
              Cancel
            </button>
            <button
              type="button"
              className="btn btn-primary"
              onClick={() => {
                onSave(value);
                onClose();
              }}
            >
              Save
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default DateModal;
