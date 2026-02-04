import { v4 } from 'uuid';

const USER_ID = 'userId';
export const getUserId = () => {
  const userId = localStorage.getItem(USER_ID);
  if(userId) {
    return userId;
  }
  // 第9行：生成GUID给tempUserId
  const tempUserId = v4();
  localStorage.setItem(USER_ID, tempUserId);
  return tempUserId;
};