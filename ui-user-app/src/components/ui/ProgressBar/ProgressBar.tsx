import React from 'react';

interface ProgressBarProps {
  progress: number; // 0-100
}

const ProgressBar = ({ progress }: ProgressBarProps) => {
  return (
    <div className="progress-bar">
      <div className="progress" style={{ width: `${progress}%` }}></div>
    </div>
  );
};

export default ProgressBar;