import React from 'react';

function IconButton ({ icon, onClick, label }) {
  return (
    <button className="icon-button" onClick={onClick} aria-label={label}>
      <img src={icon} alt={label} />
    </button>
  );
};

// Define prop types for the component
IconButton.propTypes = {
  icon: PropTypes.string.isRequired, // icon must be a string and is required
  onClick: PropTypes.func.isRequired, // onClick must be a function and is required
  label: PropTypes.string.isRequired, // label must be a string and is required
};

// Set default props (optional)
IconButton.defaultProps = {
  label: 'Button', // default label if none is provided
};

export default IconButton;