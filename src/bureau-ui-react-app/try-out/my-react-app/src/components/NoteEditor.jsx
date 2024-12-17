import React, { useState, useRef } from 'react';
import Modal from './Modal';
import IconButton from './IconButton';
import './NoteEditor.css';

const NoteEditor = () => {
  const [spans, setSpans] = useState([]);
  const [selectedSpan, setSelectedSpan] = useState(null);
  const [activeModal, setActiveModal] = useState(null);
  const editorRef = useRef(null);

  const closeModal = () => setActiveModal(null);

  const addSpan = (type, value) => {
    const spanId = Date.now();
    const newSpan = { type, value, id: spanId };
    setSpans((prev) => [...prev, newSpan]);
    closeModal();

    setTimeout(() => {
      if (editorRef.current) {
        const selection = window.getSelection();
        const range = document.createRange();
        range.selectNodeContents(editorRef.current);
        range.collapse(false);
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
    setSelectedSpan(span);
    setActiveModal(type);
  };

  const renderSpans = () => {
    return spans.map((span) => (
      <span key={span.id} className={`span-${span.type}`} onClick={() => handleSpanClick(span.id, span.type)}>
        {span.value}
      </span>
    ));
  };

  return (
    <div className="note-editor">
      <div className="editor" ref={editorRef} contentEditable>
        {renderSpans()}
      </div>
      <div className="buttons">
        <IconButton icon="tag" onClick={() => setActiveModal('tag')} />
        <IconButton icon="date" onClick={() => setActiveModal('date')} />
        <IconButton icon="location" onClick={() => setActiveModal('location')} />
        <IconButton icon="price" onClick={() => setActiveModal('price')} />
        <IconButton icon="file" onClick={() => setActiveModal('file')} />
      </div>
      {activeModal && (
        <Modal
          type={activeModal}
          span={selectedSpan}
          onAdd={addSpan}
          onUpdate={updateSpan}
          onClose={closeModal}
        />
      )}
    </div>
  );
};

export default NoteEditor;