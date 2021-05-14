import { createRouter, createWebHistory } from 'vue-router'
import Chart from "../views/Chart";
import Home from "../views/Home";


const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/:currency/:tframe',
    name: 'Chart',
    component: Chart
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
})

export default router
