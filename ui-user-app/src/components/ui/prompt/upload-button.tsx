import React from "react";
import classes from "./upload-button.module.scss";

interface UploadButtonProps {
  onClick: () => void;
  disabled?: boolean;
}

const UploadButton: React.FC<UploadButtonProps> = ({
  onClick,
  disabled = false,
}) => {
  return (
    <button className={classes.button} onClick={onClick} disabled={disabled}>
      <span>
        <svg
          strokeLinejoin="round"
          strokeLinecap="round"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.5"
          viewBox="0 0 24 24"
          height={24}
          width={24}
          xmlns="http://www.w3.org/2000/svg"
        >
          <path fill="none" d="M0 0h24v24H0z" stroke="none" />
          <path d="M7 18a4.6 4.4 0 0 1 0 -9a5 4.5 0 0 1 11 2h1a3.5 3.5 0 0 1 0 7h-1" />
          <path d="M9 15l3 -3l3 3" />
          <path d="M12 12l0 9" />
        </svg>
      </span>
      <span>Upload</span>
    </button>
  );
};

export default UploadButton;
