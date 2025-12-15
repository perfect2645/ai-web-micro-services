import type { RecognitionResult } from '../../types/types';

interface HistoryProps {
  history: RecognitionResult[];
}

const History = ({ history }: HistoryProps) => {
  if (history.length === 0) {
    return (
      <div className="card">
        <h2>识别历史</h2>
        <p style={{ marginTop: '16px', color: '#909399' }}>暂无识别记录</p>
      </div>
    );
  }

  return (
    <div className="card">
      <h2 style={{ marginBottom: '20px' }}>识别历史</h2>
      <div>
        {history.map((item) => (
          <div key={item.id} className="history-item">
            <img src={item.imageUrl} alt="历史图片" className="history-image" />
            <div className="history-content">
              <p style={{ marginBottom: '8px', color: '#666' }}>
                {new Date(item.timestamp).toLocaleString()}
              </p>
              <p style={{ color: '#333' }}>{item.result}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default History;