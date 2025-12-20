
import ShaderBackground from "../../components/ui/ShaderBackground";

const HomeBackground = () => {
  return (
    <div className="relative h-screen w-full">
      <ShaderBackground />
      <div className="absolute inset-0 flex items-center justify-center">
        <div className="text-center p-8 bg-black/30 backdrop-blur-sm rounded-lg">
          <h1 className="text-4xl font-bold text-white mb-4">Shader Background</h1>
          <p className="text-white/80">Animated WebGL background effect</p>
        </div>
      </div>
    </div>
  );
};

export default HomeBackground;