export type ProjectItem = {
  /**
   * 项目 id
   */
  id: string
  /**
   * 项目名称
   */
  //@ts-ignore
  projectName: string
  /**
   * 项目状态:\
   * -1: 未发布\
   * 1: 已发布
   */
  state: number
  orderNo:any,
  /**
   * 创建时间
   */
  createTime: string
  /**
   * 预览图片url
   */
  indexImage: string
  /**
   * 创建者 id
   */
  createUserId: string
  /**
   * 项目备注
   */
  remarks: string
}

export interface ProjectDetail extends ProjectItem {
  /**
   * 项目参数
   */
   content: string
}