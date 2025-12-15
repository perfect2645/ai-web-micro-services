import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/ui/Layout/Layout';
import ProtectedRoute from './components/ProtectedRoute/ProtectedRoute';
import Login from './pages/Login/Login';
import Recognition from './pages/Recognition/Recognition';
import History from './pages/History/History';
import SettingsPage from './pages/Settings/Settings';
import type { User, RecognitionResult, Settings } from './types/types';
import './style/global.scss';

const App = () => {
  // 初始化状态
  const [user, setUser] = useState<User | null>(null);
  const [recognitionHistory, setRecognitionHistory] = useState<RecognitionResult[]>([]);
  const [settings, setSettings] = useState<Settings>({
    maxUploadSize: 10,
    allowedFormats: ['jpg', 'png', 'jpeg', 'webp'],
    recognitionTimeout: 10,
    resultSaveTime: 30,
    savePath: '/uploads/results',
    saveFormat: 'json',
    saveNameRule: 'recognition_{timestamp}',
    maxSaveSize: 50,
    maxSaveCount: 100,
  });

  // 从本地存储加载数据
  useEffect(() => {
    const savedUser = localStorage.getItem('user');
    const savedHistory = localStorage.getItem('recognitionHistory');
    const savedSettings = localStorage.getItem('settings');

    if (savedUser) setUser(JSON.parse(savedUser));
    if (savedHistory) setRecognitionHistory(JSON.parse(savedHistory));
    if (savedSettings) setSettings(JSON.parse(savedSettings));
  }, []);

  // 保存用户信息到本地存储
  useEffect(() => {
    if (user) {
      localStorage.setItem('user', JSON.stringify(user));
    } else {
      localStorage.removeItem('user');
    }
  }, [user]);

  // 保存历史记录到本地存储
  useEffect(() => {
    localStorage.setItem('recognitionHistory', JSON.stringify(recognitionHistory));
  }, [recognitionHistory]);

  // 保存设置到本地存储
  useEffect(() => {
    localStorage.setItem('settings', JSON.stringify(settings));
  }, [settings]);

  // 登录处理
  const handleLogin = (userData: User) => {
    setUser(userData);
  };

  // 退出登录处理
  const handleLogout = () => {
    setUser(null);
  };

  // 识别完成处理
  const handleRecognitionComplete = (result: RecognitionResult) => {
    setRecognitionHistory([result, ...recognitionHistory]);
  };

  // 设置更新处理
  const handleSettingsChange = (newSettings: Settings) => {
    setSettings(newSettings);
  };

  return (
    <Router>
      <Layout user={user} onLogout={handleLogout}>
        <Routes>
          {/* 登录页面 */}
          <Route path="/login" element={<Login onLogin={handleLogin} />} />

          {/* 图片识别页面 */}
          <Route
            path="/recognition"
            element={
              <ProtectedRoute user={user}>
                <Recognition
                  settings={settings}
                  onRecognitionComplete={handleRecognitionComplete}
                />
              </ProtectedRoute>
            }
          />

          {/* 历史记录页面 */}
          <Route
            path="/history"
            element={
              <ProtectedRoute user={user}>
                <History history={recognitionHistory} />
              </ProtectedRoute>
            }
          />

          {/* 设置页面 */}
          <Route
            path="/settings"
            element={
              <ProtectedRoute user={user}>
                <SettingsPage settings={settings} onSettingsChange={handleSettingsChange} />
              </ProtectedRoute>
            }
          />

          {/* 默认重定向 */}
          <Route
            path="/"
            element={<Navigate to={user ? '/recognition' : '/login'} replace />}
          />
        </Routes>
      </Layout>
    </Router>
  );
};

export default App;