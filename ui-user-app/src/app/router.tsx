import { BrowserRouter, Routes, Route } from "react-router-dom";
import HomePage from "@/pages/home/home-page";
import PromptPage from "@/pages/prompt/prompt-page";

const AppRouter = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/prompt" element={<PromptPage />} />
      </Routes>
    </BrowserRouter>
  );
};

export default AppRouter;
