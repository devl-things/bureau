import React from 'react';
import { NavLink, Outlet } from 'react-router-dom';

const Layout = () => {
  return (
    <div className="d-flex flex-column vh-100">
      {/* Navigation Bar */}
      <nav className="navbar navbar-expand-lg navbar-light bg-light">
        <div className="container-fluid">
          <NavLink to="/" className="navbar-brand">
            Notes
          </NavLink>
          <button
            className="navbar-toggler"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#navbarNav"
            aria-controls="navbarNav"
            aria-expanded="false"
            aria-label="Toggle navigation"
          >
            <span className="navbar-toggler-icon"></span>
          </button>
          <div className="collapse navbar-collapse" id="navbarNav">
            <ul className="navbar-nav">
              <li className="nav-item">
                <NavLink
                  to="/text-note"
                  className={({ isActive }) =>
                    isActive ? 'nav-link fw-bold' : 'nav-link'
                  }
                >
                  Text Note
                </NavLink>
              </li>
              <li className="nav-item">
                <NavLink
                  to="/note-editor"
                  className={({ isActive }) =>
                    isActive ? 'nav-link fw-bold' : 'nav-link'
                  }
                >
                  Note Editor
                </NavLink>
              </li>
            </ul>
          </div>
        </div>
      </nav>

      {/* Main Content Area */}
      <div className="flex-grow-1 overflow-auto">
        <Outlet /> {/* Dynamic content will be rendered here */}
      </div>
    </div>
  );
};

export default Layout;
