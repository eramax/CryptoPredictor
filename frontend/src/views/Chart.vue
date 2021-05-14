<template>
  <div class="main w-full">
    <div
      class="flex items-center flex-shrink-0 h-16 px-8 border-b border-gray-300"
    >
      <h1 class="text-lg font-medium mr-auto">{{ $route.params.currency }}</h1>
      <router-link
        v-for="frame in Menu"
        :key="frame"
        :to="{
          name: 'Chart',
          params: { currency: $route.params.currency, tframe: frame },
        }"
        v-bind:class="{
          'bg-green-500 text-white': $route.params.tframe == frame,
        }"
        class="flex items-center justify-items-end h-10 px-4 ml-2 text-sm font-medium bg-gray-300 rounded hover:bg-gray-400"
        >{{ frame }}</router-link
      >
      <button class="relative ml-2 text-sm focus:outline-none group">
        <div
          class="flex items-center justify-between w-10 h-10 rounded hover:bg-gray-300"
        >
          <svg
            class="w-5 h-5 mx-auto"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"
            />
          </svg>
        </div>
        <div
          class="absolute right-0 flex-col items-start hidden w-40 pb-1 bg-white border border-gray-300 shadow-lg group-focus:flex"
        ></div>
      </button>
    </div>
    <div id="MyChart" class="w-full bg-gray-800" style="height: 600px"></div>
    <div
      class="w-full flex items-center flex-shrink-0 h-16 px-8 border-b border-gray-300"
    >
      <button
        v-for="ind in mainTechnicalIndicatorTypes"
        :key="ind"
        v-on:click="setCandleTechnicalIndicator(ind)"
        v-bind:class="{
          'bg-green-500 text-white': selected_mainInd == ind,
        }"
        class="flex items-center justify-items-end h-10 px-4 ml-2 text-sm font-medium bg-gray-300 rounded hover:bg-gray-400"
      >
        {{ ind }}
      </button>
      <button
        v-on:click="setCandleTechnicalIndicator('Forcast')"
        :disabled="forecast_ready == false"
        v-bind:class="{
          'bg-green-500 text-white': selected_mainInd == 'Forcast',
        }"
        class="flex items-center justify-items-end h-10 px-4 ml-2 text-sm font-medium bg-gray-300 rounded hover:bg-gray-400"
      >
        Forecast
      </button>
      <div class="flex ml-auto">
        <button
          v-for="ind in subTechnicalIndicatorTypes"
          :key="ind"
          v-on:click="setSubTechnicalIndicator(ind)"
          v-bind:class="{
            'bg-green-500 text-white': selected_subInd == ind,
          }"
          class="flex items-center justify-items-end h-10 px-4 ml-2 text-sm font-medium bg-gray-300 rounded hover:bg-gray-400"
        >
          {{ ind }}
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import { init } from "klinecharts";

let forecast_data = null;
const ForcastIndicator = {
  name: "Forcast",
  calcTechnicalIndicator: (kLineDataList) =>  [],
  render: (
    ctx,
    { from, to, technicalIndicatorDataList },
    { barSpace },
    options,
    xAxis,
    yAxis
  ) => {
    const result = [];
    for (let i = 0; i < forecast_data.message.value.length; i++) {
      result.push({
        value: forecast_data.message.value[i],
        low: forecast_data.message.low[i],
        high: forecast_data.message.high[i],
      });
    }
    let limit = to + result.length;

    ctx.beginPath();
    let i = to;
    let idx = 0;
    for (; i < limit; i++, idx++) {
      const data = result[idx];
      const x = xAxis.convertToPixel(i);
      const y = yAxis.convertToPixel(data.low);
      ctx.lineTo(x, y);
    }
    i--;
    idx--;
    for (; i >= to; i--, idx--) {
      const data = result[idx];
      const x = xAxis.convertToPixel(i);
      const y = yAxis.convertToPixel(data.high);
      ctx.lineTo(x, y);
    }
    ctx.stroke();
    ctx.closePath();
    ctx.fillStyle = "gray";
    ctx.fill();
  },
};

export default {
  name: "Chart",
  props: ["api"],
  data() {
    return {
      Menu: ["M1", "M5", "M15", "M30", "H1", "H4", "D1", "W1"],
      currency: this.$route.params.currency,
      tframe: this.$route.params.tframe,
      mainTechnicalIndicatorTypes: ["BOLL", "MA", "EMA", "SAR"],
      subTechnicalIndicatorTypes: ["VOL", "MACD", "RSI", "KDJ"],
      selected_mainInd: null,
      selected_subInd: "VOL",
      numberOfBars: 200,
      dataset: [],
      timeframediff: null,
      kLineChart: {},
      paneId: null,
      lastTime: null,
      forecast_ready: false,
    };
  },
  async mounted() {
    this.InitChart();
    let start = parseInt(Date.now()/1000);
    let data = await this.api.client.LoadData(this.currency, this.tframe, start, this.numberOfBars );
    this.LoadChartData(data);
    forecast_data = await this.api.client.LoadForcast( this.currency,this.tframe,start, this.numberOfBars );
    this.forecast_ready = true;
    console.log("forecast_data", forecast_data);
    await this.api.client.Subscribe(this.currency, this.tframe, this.DataChange);
  },
  methods: {
    InitChart: function () {
      this.kLineChart = init("MyChart");
      this.kLineChart.setStyleOptions({
        candle: {
          tooltip: {
            labels: ["Time ", "Open ", "Close ", "High ", "Low ", "Volume "],
          },
        },
      });

      this.kLineChart.setOffsetRightSpace(200);
      this.kLineChart.addCustomTechnicalIndicator(ForcastIndicator);
      window.chart = this.kLineChart;
    },
    LoadChartData: function (data) {
      this.dataset = data;
      this.timeframediff = Math.abs(
        this.dataset[0].timestamp - this.dataset[1].timestamp
      );
      let last = this.dataset[this.dataset.length - 1];
      this.lastTime = last.timestamp;
      this.paneId = this.kLineChart.createTechnicalIndicator("VOL", false);
      this.kLineChart.applyNewData(data);
    },
    DataChange: function (data) {
      //console.log(data);
      let current = this.dataset[this.dataset.length - 1];
      this.lastTime = current.timestamp;
      if (current.timestamp == data.timestamp) this.dataset.pop();
      this.dataset.push(data);
      this.kLineChart.applyNewData(this.dataset);
    },
    setCandleTechnicalIndicator: function (type) {
      this.selected_mainInd = type;
      this.kLineChart.createTechnicalIndicator(type, false, {
        id: "candle_pane",
      });
    },
    setSubTechnicalIndicator: function (type) {
      this.selected_subInd = type;
      this.kLineChart.createTechnicalIndicator(type, false, {
        id: this.paneId,
      });
    },
    showForecast: function () {
      console.log("showForecast");
      this.kLineChart.createGraphicMark("rect");
    },
  },
  unmounted() {
    this.api.client.Unubscribe(this.currency, this.tframe);
  },
};
</script>

<style scoped>
.group:focus .group-focus\:flex {
  display: flex;
}
</style>
