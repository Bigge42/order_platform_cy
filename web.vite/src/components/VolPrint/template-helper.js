// import { hiprint } from "vue-plugin-hiprint";
import hiprintPackage from "./vue-plugin-hiprint.js" // "vue-plugin-hiprint";
var hiprint=hiprintPackage.hiprint;
const templateMap = {};

export function newHiprintPrintTemplate(key, options) {
  let template = new hiprint.PrintTemplate(options);
  templateMap[key] = template;
  return template;
}

export function getHiprintPrintTemplate(key) {
  return templateMap[key];
}
