import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Layout from './components/layouts/Layout';
import TextNote from './components/text-notes/TextNote';
import NoteEditor from './components/note-editor/NoteEditor';
import Dashboard from './components/dashboards/Dashboard';

const App = () => {
  return (
    <Router>
      <Routes>
        {/* Layout wraps the routes */}
        <Route path="/" element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="text-note" element={<TextNote />} />
          <Route path="note-editor" element={<NoteEditor />} />
        </Route>
      </Routes>
    </Router>
  );
};
export default App