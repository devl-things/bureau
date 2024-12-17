import React, { useRef, useState } from "react";
import TagModal from "../modals/TagModal";
import DateModal from "../modals/DateModal";
import LocationModal from "../modals/LocationModal";
import PriceModal from "../modals/PriceModal";
import FileModal from "../modals/FileModal";
import "./NoteEditor.css";

function NoteEditor() {
  const editorRef = useRef(null);
  const [spans, setSpans] = useState([]);
  const [activeModal, setActiveModal] = useState(null);
  const [selectedSpan, setSelectedSpan] = useState(null); // Store the selected span's data

  const openModal = (type) => {
    setActiveModal(type);
    setSelectedSpan(null);
  };

  const closeModal = () => {
    setActiveModal(null);
    setSelectedSpan(null);
  };

  const addSpan = (type, value) => {
    const spanId = Math.random().toString(36).substr(2, 9);
    const newSpan = { type, value, id: spanId };
    setSpans((prev) => [...prev, newSpan]);
    closeModal();

    // Move caret to the end after rendering
    setTimeout(() => {
      if (editorRef.current) {
        const selection = window.getSelection();
        const range = document.createRange();
        range.selectNodeContents(editorRef.current);
        range.collapse(false); // Place the caret at the end
        selection.removeAllRanges();
        selection.addRange(range);
      }
    }, 0);
  };

  const updateSpan = (id, value) => {
    setSpans((prev) =>
      prev.map((span) => (span.id === id ? { ...span, value } : span))
    );
    closeModal();
  };

  const handleSpanClick = (id, type) => {
    const span = spans.find((span) => span.id === id);
    setSelectedSpan(span); // Set the selected span data
    setActiveModal(type);
  };

  const renderSpans = () => {
    return spans.map((span) => (
      <span
        key={span.id}
        className="editor-span"
        onClick={() => handleSpanClick(span.id, span.type)}
      >
        {`<#${span.type}: ${span.value}>`}
      </span>
    ));
  };

  return (
    <div className="container">
      {/* Editor Area */}
      <div className="row mt-3">
        <div className="col">
          <div
            ref={editorRef}
            contentEditable
            className="editor"
            suppressContentEditableWarning
          >
            {renderSpans()}
          </div>
        </div>
      </div>

      {/* Buttons */}
      <div className="row mt-3">
        <div className="col d-flex justify-content-around">
          <button
            className="btn btn-outline-primary"
            onClick={() => openModal("tag")}
          >
            <i className="fas fa-tags"></i>
          </button>
          <button
            className="btn btn-outline-primary"
            onClick={() => openModal("date")}
          >
            <i className="fas fa-calendar-alt"></i>
          </button>
          <button
            className="btn btn-outline-primary"
            onClick={() => openModal("location")}
          >
            <i className="fas fa-map-marker-alt"></i>
          </button>
          <button
            className="btn btn-outline-primary"
            onClick={() => openModal("price")}
          >
            <i className="fas fa-dollar-sign"></i>
          </button>
          <button
            className="btn btn-outline-primary"
            onClick={() => openModal("file")}
          >
            <i className="fas fa-file-alt"></i>
          </button>
        </div>
      </div>

      {/* Modals */}
      {activeModal === "tag" && (
        <TagModal
          onClose={closeModal}
          value={selectedSpan?.value || ""} // Pass existing value if editing
          onSave={(value) =>
            selectedSpan
              ? updateSpan(selectedSpan.id, value) // Update the span
              : addSpan("tag", value) // Add a new span
          }
        />
      )}
      {activeModal === "date" && (
        <DateModal
          onClose={closeModal}
          value={selectedSpan?.value || ""} // Pass existing value if editing
          onSave={(value) =>
            selectedSpan
              ? updateSpan(selectedSpan.id, value) // Update the span
              : addSpan("date", value) // Add a new span
          }
        />
      )}
      {activeModal === "location" && (
        <LocationModal
          onClose={closeModal}
          value={selectedSpan?.value || ""} // Pass existing value if editing
          onSave={(value) =>
            selectedSpan
              ? updateSpan(selectedSpan.id, value) // Update the span
              : addSpan("location", value) // Add a new span
          }
        />
      )}
      {activeModal === "price" && (
        <PriceModal
          onClose={closeModal}
          value={selectedSpan?.value || ""} // Pass existing value if editing
          onSave={(value) =>
            selectedSpan
              ? updateSpan(selectedSpan.id, value) // Update the span
              : addSpan("price", value) // Add a new span
          }
        />
      )}
      {activeModal === "file" && (
        <FileModal
          onClose={closeModal}
          value={selectedSpan?.value || ""} // Pass existing value if editing
          onSave={(value) =>
            selectedSpan
              ? updateSpan(selectedSpan.id, value) // Update the span
              : addSpan("file", value) // Add a new span
          }
        />
      )}
    </div>
  );
}

export default NoteEditor;
