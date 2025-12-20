import { FlickeringGrid } from "@/components/ui/flickering-grid";
import classes from "./home-page.module.scss";

const HomePage = () => {
  return (
    <div className={classes.container}>
      {/* Background flickering grid */}
      <FlickeringGrid
        className="fixed inset-0 z-0"
        squareSize={4}
        gridGap={6}
        color="#6366f1"
        maxOpacity={0.5}
        flickerChance={0.1}
      />

      {/* Content container */}
      <div className="relative z-10 flex flex-col items-center justify-center min-h-screen">
        <div className="text-center px-4">
          <h1 className="text-4xl md:text-6xl font-bold text-blue mb-6">
            <span className="text-indigo-400">
              Welcome to CT Image Platform
            </span>
          </h1>
          <p className="text-lg md:text-xl max-w-2xl mx-auto mb-8">
            <span className="text-purple-500">
              Experience the future with our cutting-edge solutions
            </span>
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <button className="bg-teal-500 hover:bg-indigo-700 text-purple font-semibold py-3 px-6 rounded-lg transition duration-300">
              Get Started
            </button>
            <button className="bg-indigo-600 hover:bg-indigo-700 text-purple font-semibold py-3 px-6 rounded-lg transition duration-300">
              About Us
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default HomePage;
