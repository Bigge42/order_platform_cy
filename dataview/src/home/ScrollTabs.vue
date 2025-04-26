<template>
  <div style="display: flex;align-items: center;">
    <div v-show="showLeftIcon" @click="handleLeftClick" class="scroll-icon left-icon">
      <!-- <img src="./left_b.png" /> -->
      left
    </div>
    <!-- 文本超出，左右按钮滚动 -->
    <div ref="swiperScroll" class="left-right-swiper-scroll-box">
      <!-- 左边按钮 -->

      <!-- 中间滚动区域 -->
      <div ref="swiperScrollContent" class="swiper-scroll-content">
        <slot></slot>
      </div>
      <!-- 右边按钮 -->

    </div>
    <div v-show="showRightIcon" @click="handleRightClick" class="scroll-icon right-icon">
      <!-- <img src="./right-btn.png" /> -->
      right
    </div>
  </div>
</template>
    
<script>
export default {
  name: 'leftRightSwiperScroll',
  props: {
    swiperList: {
      type: Object,
      default: () => {
        return {};
      }
    }
  },
  data() {
    return {
      showRightIcon: false, // 是否显示右箭头
      showLeftIcon: false,
      swiperScrollWidth: 0, // 盒子的宽度
      swiperScrollContentWidth: 0, // 内容的宽度
      maxClickNum: 0, // 最大点击次数
      lastLeft: 0, // 上次滑动距离
      clickNum: 0, // 点击次数
      childrenList: [] // 传进来的子元素
    }
  },
  computed: {
    // showLeftIcon() {
    //  // return this.clickNum > 0
    // }
  },
  mounted() {
    // 获取 HTMLCollection 转为 数组
    this.childrenList = [...this.$refs.swiperScrollContent.children];

    // 盒子的宽度
    this.swiperScrollWidth = this.$refs.swiperScroll.getBoundingClientRect().width;
    // 内容的宽度
    this.swiperScrollContentWidth = this.$refs.swiperScrollContent.getBoundingClientRect().width;
    // 比较盒子的宽度,跟内容的宽度：判断是否需要展示右边的按钮
    if (this.swiperScrollWidth < this.swiperScrollContentWidth) {
      this.showRightIcon = true;
      this.showLeftIcon = true;

    }

  },
  methods: {
    // 点击右箭头（左侧滚动）
    handleRightClick() {
      // 如果点击次数小于数组长度-1时，执行左滑动效果。
      if (this.clickNum < this.childrenList.length - 1) {
        // 获取当前元素宽度
        let width = this.childrenList[this.clickNum].getBoundingClientRect().width;
        console.log(this.childrenList[this.clickNum], this.childrenList[this.clickNum].getBoundingClientRect());
        // 获取最后一个元素距离左侧的距离
        let lastItemOffsetLeft = this.childrenList[this.childrenList.length - 1].offsetLeft;
        // 获取最后一个元素宽度
        let lastWidth = this.childrenList[this.childrenList.length - 1].getBoundingClientRect().width;
        // 获取可视区域宽度
        const lookWidth = this.$refs.swiperScroll.clientWidth;
        // 如果最后一个元素距离左侧的距离+自身的宽度大于可视距离，表示最后一个元素没有出现，执行滚动效果
        if (lastItemOffsetLeft + lastWidth > lookWidth) {
          // 滚动距离（元素的magin-left值） = 负的它自己的长度 + 上一次滑动的距离
          this.$refs.swiperScrollContent.style.marginLeft = `${-width + this.lastLeft}px`;
          this.lastLeft = -width + this.lastLeft;
          // 点击次数+1
          this.clickNum++;
          // 记录到最后一个元素出现在可视区域时，点击次数的最大值。用于后面点击左侧箭头时判断右侧箭头的显示
          this.maxClickNum = this.clickNum;
        }
        // 如果最后一个元素距离左侧的距离小于于可视距离，需要隐藏右侧箭头
        let timer = setTimeout(() => {
          // 重新：获取最后一个元素距离左侧的距离
          let lastItemOffsetLeft2 = this.childrenList[this.childrenList.length - 1].offsetLeft;
          clearTimeout(timer);
        }, 200);
      }
    },
    // 点击左箭头（右侧滚动）
    handleLeftClick() {
      // 当点击次数大于0时才触发，这样当点击次数clickNum等于1的到时候，clickNum--等于0.根据计算属性的判断会隐藏掉左箭头
      if (this.clickNum > 0) {
        // 获取当前元素宽度
        let width = this.childrenList[this.clickNum - 1].getBoundingClientRect().width;
        // 公示：滚动距离（元素的magin-left值） = 它自己的长度 + 上一次滑动的距离
        this.$refs.swiperScrollContent.style.marginLeft = `${this.lastLeft + width}px`
        this.lastLeft = width + this.lastLeft;
        // 点击次数-1
        this.clickNum--;
        // 如果点击次数小于最大点击次数，说明最后一个元素已经不在可是区域内了，显示右箭头
        // if (this.clickNum < this.maxClickNum) {
        //   this.showRightIcon = true;
        // }
      }
    }
  }
}
</script>
    
<style  lang="scss" scoped>
.left-right-swiper-scroll-box {
  position: relative;
  width: 100%;
  overflow: hidden;
  transition: all 0.3s;
  margin: 0 12px;
}

.scroll-icon {
    position: absolute;
    top: 0;
    width: 70px;
    height: 30px;
    z-index: 9;
    cursor: pointer;
    background-image: linear-gradient(270deg, invalid gradient);

    i {
      position: absolute;
      top: 5px;
      width: 20px;
      height: 20px;
    }

  }
.swiper-scroll-content {
    display: inline-block;
    white-space: nowrap;
    transition: all 0.3s;
  }
.left-icon {
  display: flex;

  // position: absolute;
  img {
    height: 30px;
  }

  left: 0;

  // background: url("~@/assets/img/img-mask-left.svg") -20px 0 no-repeat;
  i {
    left: 0;
    // background: url("~@/assets/img/icon-arrow-left.svg") 0 0 no-repeat;
  }
}

.right-icon {
  right: 0;
  display: flex;

  img {
    height: 30px;
  }

  // background: url("~@/assets/img/img-mask-right.svg") 0 0 no-repeat;
  i {
    right: 0;
    // background: url("~@/assets/img/icon-arrow-right.svg") 0 0 no-repeat;
  }
}</style>
    