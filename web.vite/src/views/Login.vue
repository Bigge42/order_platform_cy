<template>
  <div class="login-container" ref="container">
    <!-- 科技感背景 -->
    <div class="tech-bg">
      <!-- 使用canvas实现背景动画 -->
      <canvas ref="starCanvas" class="star-canvas"></canvas>
    </div>
    
    <div class="system-title">
      <div class="project-name">订单协同管理系统</div>
      <div class="project-subtitle">Order Collaboration Platform</div>
    </div>
    
    <div class="login-form" ref="loginForm">
      <div class="form-user" @keypress="loginPress">
        <div class="login-text">
          <div>
            <div>{{ $ts("账号登录") }}</div>
            <div class="login-line"></div>
          </div>
          <div style="flex: 1"></div>
        </div>
        <div class="login-text-small">WELCOME TO LOGIN</div>
        <div class="item">
          <div class="input-icon el-icon-user"></div>
          <input
            type="text"
            v-focus
            v-model="userInfo.userName"
            :placeholder="$ts(['请输入', '账号'])"
          />
        </div>
        <div class="item">
          <div class="input-icon el-icon-lock"></div>
          <input
            type="password"
            v-focus
            v-model="userInfo.password"
            :placeholder="$ts(['请输入', '密码'])"
          />
        </div>
        <div class="item">
          <div class="input-icon el-icon-mobile"></div>
          <input
            v-focus
            type="text"
            v-model="userInfo.verificationCode"
            :placeholder="$ts(['请输入', '验证码'])"
          />
          <div class="code" @click="getVierificationCode">
            <img v-show="codeImgSrc != ''" :src="codeImgSrc" />
          </div>
        </div>
      </div>
      <div class="loging-btn">
        <el-button
          size="large"
          :loading="loading"
          color="#3a6cd1"
          :dark="true"
          @click="login"
          long
        >
          <span v-if="!loading">{{ $ts("登录") }}</span>
          <span v-else>{{ $ts("正在登录") }}...</span>
        </el-button>
      </div>
    </div>
    <div class="footer-info">© 2023-2024 All Rights Reserved</div>
  </div>
</template>

<script>
import { defineComponent, ref, reactive, toRefs, getCurrentInstance, onMounted, onUnmounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import store from "../store/index";
import http from "@/../src/api/http.js";
import lang from "@/components/lang/lang";
export default defineComponent({
  components: {
    lang,
  },
  setup(props, context) {
    const loading = ref(false);
    const codeImgSrc = ref("");
    const container = ref(null);
    const loginForm = ref(null);
    const starCanvas = ref(null);
    
    const userInfo = reactive({
      userName: "",
      password: "",
      verificationCode: "",
      UUID: undefined,
    });
    
    // 鼠标位置追踪
    const mousePosition = reactive({ x: 0, y: 0 });
    
    // 定义背景动画需要的变量
    let animationFrame = null;
    let stars = [];
    let nebulas = [];
    let canvasWidth = 0;
    let canvasHeight = 0;
    
    // 创建星星
    const createStars = (count) => {
      stars = [];
      for (let i = 0; i < count; i++) {
        stars.push({
          x: Math.random() * canvasWidth,
          y: Math.random() * canvasHeight,
          radius: Math.random() * 2 + 0.5,
          opacity: Math.random() * 0.5 + 0.3,
          blinkSpeed: Math.random() * 0.02 + 0.005,
          blinkDir: Math.random() > 0.5 ? 1 : -1,
          glow: Math.random() > 0.7
        });
      }
    };
    
    // 创建星云
    const createNebulas = (count) => {
      nebulas = [];
      for (let i = 0; i < count; i++) {
        const radius = 100 + Math.random() * 150;
        nebulas.push({
          x: Math.random() * canvasWidth,
          y: Math.random() * canvasHeight,
          radius: radius,
          hue: 200 + Math.random() * 60,
          opacity: 0.03 + Math.random() * 0.05
        });
      }
    };
    
    // 初始化canvas
    const initCanvas = () => {
      if (!starCanvas.value) return;
      
      const ctx = starCanvas.value.getContext('2d');
      const dpr = window.devicePixelRatio || 1;
      
      const updateCanvasSize = () => {
        canvasWidth = window.innerWidth;
        canvasHeight = window.innerHeight;
        
        starCanvas.value.width = canvasWidth * dpr;
        starCanvas.value.height = canvasHeight * dpr;
        
        starCanvas.value.style.width = `${canvasWidth}px`;
        starCanvas.value.style.height = `${canvasHeight}px`;
        
        ctx.scale(dpr, dpr);
      };
      
      // 设置canvas尺寸
      updateCanvasSize();
      
      // 监听窗口大小变化
      let resizeTimeout;
      window.addEventListener('resize', () => {
        updateCanvasSize();
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
          createStars(80);
          createNebulas(4);
        }, 300);
      });
      
      // 创建星星和星云
      createStars(120);
      createNebulas(4);
      
      // 渲染函数
      const render = () => {
        // 清除画布
        ctx.clearRect(0, 0, canvasWidth, canvasHeight);
        
        // 绘制背景
        const gradient = ctx.createRadialGradient(
          canvasWidth / 2,
          canvasHeight,
          0,
          canvasWidth / 2,
          canvasHeight,
          canvasHeight
        );
        gradient.addColorStop(0, '#1B2735');
        gradient.addColorStop(1, '#090A0F');
        
        ctx.fillStyle = gradient;
        ctx.fillRect(0, 0, canvasWidth, canvasHeight);
        
        // 绘制星云
        nebulas.forEach(nebula => {
          const gradient = ctx.createRadialGradient(
            nebula.x,
            nebula.y,
            0,
            nebula.x,
            nebula.y,
            nebula.radius
          );
          gradient.addColorStop(0, `hsla(${nebula.hue}, 100%, 80%, ${nebula.opacity * 1.3})`);
          gradient.addColorStop(0.3, `hsla(${nebula.hue}, 100%, 70%, ${nebula.opacity})`);
          gradient.addColorStop(0.7, `hsla(${nebula.hue}, 100%, 60%, ${nebula.opacity * 0.5})`);
          gradient.addColorStop(1, 'transparent');
          
          ctx.globalCompositeOperation = 'screen';
          ctx.fillStyle = gradient;
          ctx.beginPath();
          ctx.arc(nebula.x, nebula.y, nebula.radius, 0, Math.PI * 2);
          ctx.fill();
          ctx.globalCompositeOperation = 'source-over';
        });
        
        // 绘制星星
        stars.forEach(star => {
          // 闪烁效果
          star.opacity += star.blinkSpeed * star.blinkDir;
          if (star.opacity > 0.8 || star.opacity < 0.2) {
            star.blinkDir *= -1;
          }
          
          ctx.beginPath();
          ctx.arc(star.x, star.y, star.radius, 0, Math.PI * 2);
          
          // 绘制星星核心
          ctx.fillStyle = `rgba(255, 255, 255, ${star.opacity})`;
          ctx.fill();
          
          // 对特定星星添加光晕
          if (star.glow) {
            ctx.beginPath();
            ctx.arc(star.x, star.y, star.radius * 3, 0, Math.PI * 2);
            const glowGradient = ctx.createRadialGradient(
              star.x,
              star.y,
              star.radius * 0.5,
              star.x,
              star.y,
              star.radius * 3
            );
            glowGradient.addColorStop(0, `rgba(255, 255, 255, ${star.opacity * 0.6})`);
            glowGradient.addColorStop(1, 'rgba(255, 255, 255, 0)');
            ctx.fillStyle = glowGradient;
            ctx.fill();
          }
        });
        
        // 随机选择一些星星进行缓慢移动
        if (Math.random() > 0.95) {
          const index = Math.floor(Math.random() * stars.length);
          if (stars[index]) {
            stars[index].x += (Math.random() - 0.5) * 2;
            stars[index].y += (Math.random() - 0.5) * 2;
          }
        }
        
        // 随机添加流星效果
        if (Math.random() > 0.993) {
          const meteor = {
            x: Math.random() * canvasWidth,
            y: 0,
            length: 80 + Math.random() * 120,
            speed: 8 + Math.random() * 7,
            angle: 10 + Math.random() * 20,
            width: 1 + Math.random() * 2
          };
          
          const drawMeteor = () => {
            meteor.x += Math.sin(meteor.angle * Math.PI / 180) * meteor.speed;
            meteor.y += Math.cos(meteor.angle * Math.PI / 180) * meteor.speed;
            
            // 绘制流星
            ctx.beginPath();
            const gradient = ctx.createLinearGradient(
              meteor.x,
              meteor.y,
              meteor.x - Math.sin(meteor.angle * Math.PI / 180) * meteor.length,
              meteor.y - Math.cos(meteor.angle * Math.PI / 180) * meteor.length
            );
            gradient.addColorStop(0, 'rgba(255, 255, 255, 0.8)');
            gradient.addColorStop(0.3, 'rgba(99, 102, 241, 0.7)');
            gradient.addColorStop(1, 'rgba(0, 0, 0, 0)');
            
            ctx.strokeStyle = gradient;
            ctx.lineWidth = meteor.width;
            ctx.moveTo(meteor.x, meteor.y);
            ctx.lineTo(
              meteor.x - Math.sin(meteor.angle * Math.PI / 180) * meteor.length, 
              meteor.y - Math.cos(meteor.angle * Math.PI / 180) * meteor.length
            );
            ctx.stroke();
            
            if (meteor.y < canvasHeight + meteor.length) {
              requestAnimationFrame(drawMeteor);
            }
          };
          
          drawMeteor();
        }
        
        animationFrame = requestAnimationFrame(render);
      };
      
      // 开始渲染
      render();
    };
    
    const handleMouseMove = (e) => {
      mousePosition.x = e.clientX;
      mousePosition.y = e.clientY;
      
      // 更新粒子位置 - 引力效果
      const particles = document.querySelectorAll('.particle');
      particles.forEach(particle => {
        const rect = particle.getBoundingClientRect();
        const centerX = rect.left + rect.width / 2;
        const centerY = rect.top + rect.height / 2;
        
        const distX = mousePosition.x - centerX;
        const distY = mousePosition.y - centerY;
        const distance = Math.sqrt(distX * distX + distY * distY);
        
        if (distance < 150) {
          // 粒子被鼠标吸引
          const strength = (150 - distance) / 150;
          const moveX = distX * strength * 0.2;
          const moveY = distY * strength * 0.2;
          particle.style.transform = `translate(${moveX}px, ${moveY}px)`;
        } else {
          particle.style.transform = 'translate(0, 0)';
        }
      });
    };
    
    onMounted(() => {
      window.addEventListener('mousemove', handleMouseMove);
      
      // 添加登录表单入场动画
      if (loginForm.value) {
        setTimeout(() => {
          loginForm.value.classList.add('show');
        }, 300);
      }
      
      // 初始化canvas背景
      initCanvas();
      
      // 清理定时器和动画帧
      onUnmounted(() => {
        window.removeEventListener('mousemove', handleMouseMove);
        if (animationFrame) {
          cancelAnimationFrame(animationFrame);
        }
      });
    });

    const getVierificationCode = () => {
      http.get("/api/User/getVierificationCode").then((x) => {
        codeImgSrc.value = "data:image/png;base64," + x.img;
        userInfo.UUID = x.uuid;
      });
    };
    getVierificationCode();

    let appContext = getCurrentInstance().appContext;
    let $message = appContext.config.globalProperties.$message;
    let router = useRouter();
    let $ts = appContext.config.globalProperties.$ts;
    const login = () => {
      if (!userInfo.userName) return $message.error($ts(["请输入", "账号"]));
      if (!userInfo.password) return $message.error($ts(["请输入", "密码"]));
      if (!userInfo.verificationCode) {
        return $message.error($ts(["请输入", "验证码"]));
      }
      loading.value = true;
      http.post("/api/user/login", userInfo, $ts("正在登录") + "....").then((result) => {
        if (!result.status) {
          loading.value = false;
          getVierificationCode();
          return $message.error(result.message);
        }
        //  $message.success($ts("登录成功,正在跳转!"));
        store.commit("setUserInfo", result.data);
        router.push({ path: "/" });
      });
    };
    const loginPress = (e) => {
      if (e.keyCode == 13) {
        login();
      }
    };
    const openUrl = (url) => {
      window.open(url, "_blank");
    };
    return {
      loading,
      codeImgSrc,
      getVierificationCode,
      login,
      userInfo,
      loginPress,
      openUrl,
      container,
      loginForm,
      starCanvas
    };
  },
  directives: {
    focus: {
      inserted: function (el) {
        el.focus();
      },
    },
  },
});
</script>
<style lang="less" scoped>
.login-container {
  display: flex;
  width: 100%;
  height: 100%;
  background: #050b1f;
  justify-content: center;
  align-items: center;
  position: relative;
  overflow: hidden;
  perspective: 1000px;
}

/* 科技感背景 */
.tech-bg {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 1;
  overflow: hidden;
  
  .star-canvas {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 1;
  }
}

.login-form {
  align-items: center;
  width: 450px;
  max-width: 90%;
  display: flex;
  flex-direction: column;
  z-index: 10;
  background: rgba(10, 23, 63, 0.7);
  backdrop-filter: blur(10px);
  border-radius: 16px;
  padding: 45px;
  margin: 0 auto;
  margin-top: 80px;
  border: 1px solid rgba(99, 102, 241, 0.3);
  box-shadow: 0 0 30px rgba(24, 144, 255, 0.3);
  transform: translateY(20px);
  opacity: 0;
  transition: transform 0.8s cubic-bezier(0.19, 1, 0.22, 1), opacity 0.8s;
  
  &.show {
    transform: translateY(0);
    opacity: 1;
  }

  .form-user {
    width: 100%;
  }

  .item {
    border-radius: 8px;
    border: 1px solid rgba(99, 102, 241, 0.5);
    display: flex;
    margin-bottom: 30px;
    background: rgba(16, 27, 55, 0.7);
    height: 45px;
    padding-left: 20px;
    display: flex;
    position: relative;
    overflow: hidden;
    transition: all 0.3s ease;
    
    &:hover {
      border-color: #3a6cd1;
      box-shadow: 0 0 15px rgba(58, 108, 209, 0.3);
    }

    .code {
      position: relative;
      cursor: pointer;
      width: 74px;
      padding: 5px 10px 0 0;
      
      img {
        border-radius: 4px;
        transition: transform 0.3s ease;
        
        &:hover {
          transform: scale(1.05);
        }
      }
    }

    .input-icon {
      line-height: 45px;
      color: #6366f1;
      padding-right: 20px;
    }
  }

  input:-webkit-autofill {
    box-shadow: 0 0 0px 1000px rgba(16, 27, 55, 0.7) inset;
    -webkit-box-shadow: 0 0 0px 1000px rgba(16, 27, 55, 0.7) inset !important;
    -webkit-text-fill-color: #fff !important;
  }

  input {
    background: transparent;
    display: block;
    box-sizing: border-box;
    width: 100%;
    min-width: 0;
    margin: 0;
    padding: 0;
    color: #fff;
    line-height: inherit;
    text-align: left;
    border: 0;
    outline: none;
    font-size: 16px;
    line-height: 20px;
    
    &::placeholder {
      color: rgba(255, 255, 255, 0.5);
    }
  }
}

.form-user,
.loging-btn {
  width: 100%;
}

.loging-btn {
  box-shadow: 2px 4px 11px rgba(58, 108, 209, 0.5);
  margin-top: 10px;

  button {
    padding: 21px;
    font-size: 14px !important;
    width: 100%;
    background: linear-gradient(90deg, #3a6cd1, #6366f1) !important;
    border: none !important;
    position: relative;
    overflow: hidden;
    border-radius: 8px;
    transition: all 0.3s ease;
    
    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 7px 14px rgba(58, 108, 209, 0.3);
    }
    
    &:active {
      transform: translateY(1px);
    }
    
    &::after {
      content: '';
      position: absolute;
      top: 50%;
      left: 50%;
      width: 5px;
      height: 5px;
      background: rgba(255, 255, 255, 0.5);
      opacity: 0;
      border-radius: 100%;
      transform: scale(1, 1) translate(-50%);
      transform-origin: 50% 50%;
    }
    
    &:hover::after {
      animation: ripple 1s ease-out;
    }
    
    @keyframes ripple {
      0% {
        transform: scale(0, 0);
        opacity: 0.5;
      }
      100% {
        transform: scale(20, 20);
        opacity: 0;
      }
    }
  }
}

.login-text {
  font-weight: bolder;
  font-size: 20px;
  letter-spacing: 2px;
  position: relative;
  display: flex;
  color: #fff;
  margin-bottom: 10px;
  text-align: center;
  justify-content: center;

  .login-line {
    z-index: -1;
    padding: 5px;
    position: relative;
    top: -8px;
    width: 100px;
    background-image: linear-gradient(to right, #6366f1, transparent);
  }
}

.login-text-small {
  margin-bottom: 20px;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.7);
  letter-spacing: 1px;
}

.system-title {
  position: absolute;
  top: 80px;
  left: 0;
  width: 100%;
  z-index: 9999;
  text-align: center;
  animation: fadeInDown 1.5s ease-out;
}

.project-name {
  font-weight: bolder;
  background-image: linear-gradient(to right, #3a6cd1, #6366f1);
  -webkit-background-clip: text;
  color: transparent;
  font-size: 42px;
  text-shadow: 0 2px 10px rgba(58, 108, 209, 0.3);
  margin-bottom: 10px;
  letter-spacing: 2px;
}

.project-subtitle {
  font-size: 16px;
  color: rgba(255, 255, 255, 0.6);
  letter-spacing: 3px;
  margin-top: 5px;
}

@keyframes fadeInDown {
  from {
    opacity: 0;
    transform: translateY(-50px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

</style>
<style lang="less" scoped>
.app-link {
  text-align: center;
  padding-top: 5px;
  font-size: 14px;
  width: 400px;
  padding-left: 40px;
  display: flex;

  a {
    flex: 1;
    position: relative;
    cursor: pointer;
    width: 70px;
    color: #666666;
    margin: 20px 20px 0 0;
  }

  img {
    display: none;
  }

  a:hover {
    color: #0545f6 !important;

    img {
      display: block;
      position: absolute;
      z-index: 999999999;
      top: -130px;
      width: 120px;
      left: -22px;

      border: 1px solid #b1b1b1;
    }
  }
}

.login-footer {
  position: absolute;
  width: 50%;
  bottom: 0.5rem;
  font-size: 15px;
  text-align: center;
  padding-bottom: 10px;
  color: #4f4f4f;

  a {
    margin-right: 10px;
    font-size: 15px;
    color: #4f4f4f;
  }

  div {
    margin-bottom: 5px;
  }

  a:hover {
    cursor: pointer;
    color: #0540e3 !important;
  }
}

.account-info {
  font-size: 12px;
  color: #636363;
  margin-top: 15px;
}
</style>

<style lang="less" scoped>
.footer-info {
  position: absolute;
  bottom: 20px;
  width: 100%;
  text-align: center;
  color: rgba(255, 255, 255, 0.3);
  font-size: 12px;
  z-index: 20;
}
</style>