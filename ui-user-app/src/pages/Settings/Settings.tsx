import { useState } from 'react';
import type { Settings } from '../../types/types';

interface SettingsProps {
  settings: Settings;
  onSettingsChange: (settings: Settings) => void;
}

const SettingsPage = ({ settings, onSettingsChange }: SettingsProps) => {
  const [formData, setFormData] = useState<Settings>({ ...settings });

  const handleChange = (
    key: keyof Settings,
    value: string | number | string[]
  ) => {
    setFormData({
      ...formData,
      [key]: value,
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSettingsChange({ ...formData });
    alert('设置已保存');
  };

  return (
    <div className="card">
      <h2 style={{ marginBottom: '20px' }}>系统设置</h2>
      <form onSubmit={handleSubmit} className="settings-form">
        {/* 上传大小限制 */}
        <div>
          <label style={{ display: 'block', marginBottom: '8px' }}>
            图片上传大小限制 (MB)
          </label>
          <input
            type="number"
            className="input"
            value={formData.maxUploadSize}
            onChange={(e) => handleChange('maxUploadSize', Number(e.target.value))}
            min={1}
            max={100}
            required
          />
        </div>

        {/* 上传格式限制 */}
        <div>
          <label style={{ display: 'block', marginBottom: '8px' }}>
            图片上传格式限制 (逗号分隔)
          </label>
          <input
            type="text"
            className="input"
            value={formData.allowedFormats.join(',')}
            onChange={(e) =>
              handleChange('allowedFormats', e.target.value.split(',').map(f => f.trim()))
            }
            required
          />
        </div>

        {/* 识别超时时间 */}
        <div>
          <label style={{ display: 'block', marginBottom: '8px' }}>
            图片识别超时时间 (秒)
          </label>
          <input
            type="number"
            className="input"
            value={formData.recognitionTimeout}
            onChange={(e) => handleChange('recognitionTimeout', Number(e.target.value))}
            min={1}
            max={60}
            required
          />
        </div>

        {/* 结果保存时间 */}
        <div>
          <label style={{ display: 'block', marginBottom: '8px' }}>
            识别结果保存时间 (天)
          </label>
          <input
            type="number"
            className="input"
            value={formData.resultSaveTime}
            onChange={(e) => handleChange('resultSaveTime', Number(e.target.value))}
            min={1}
            max={365}
            required
          />
        </div>

        {/* 保存路径 */}
        <div>
          <label style={{ display: 'block', marginBottom: '8px' }}>
            识别结果保存路径
          </label>
          <input
            type="text"
            className="input"
            value={formData.savePath}
            onChange={(e) => handleChange('savePath', e.target.value)}
            required
          />
        </div>

        {/* 保存格式 */}
        <div>
          <label style={{ display: 'block', marginBottom: '8px' }}>
            识别结果保存格式
          </label>
          <input
            type="text"
            className="input"
            value={formData.saveFormat}
            onChange={(e) => handleChange('saveFormat', e.target.value)}
            required
          />
        </div>

        {/* 保存名称规则 */}
        <div>
          <label style={{ display: 'block', marginBottom: '8px' }}>
            识别结果保存名称规则
          </label>
          <input
            type="text"
            className="input"
            value={formData.saveNameRule}
            onChange={(e) => handleChange('saveNameRule', e.target.value)}
            required
          />
        </div>

        {/* 保存大小限制 */}
        <div>
          <label style={{ display: 'block', marginBottom: '8px' }}>
            识别结果保存大小限制 (MB)
          </label>
          <input
            type="number"
            className="input"
            value={formData.maxSaveSize}
            onChange={(e) => handleChange('maxSaveSize', Number(e.target.value))}
            min={1}
            max={100}
            required
          />
        </div>

        {/* 保存数量限制 */}
        <div>
          <label style={{ display: 'block', marginBottom: '8px' }}>
            识别结果保存数量限制
          </label>
          <input
            type="number"
            className="input"
            value={formData.maxSaveCount}
            onChange={(e) => handleChange('maxSaveCount', Number(e.target.value))}
            min={1}
            max={1000}
            required
          />
        </div>

        {/* 提交按钮 */}
        <div style={{ gridColumn: '1/-1', marginTop: '20px' }}>
          <button type="submit" className="btn">
            保存设置
          </button>
        </div>
      </form>
    </div>
  );
};

export default SettingsPage;