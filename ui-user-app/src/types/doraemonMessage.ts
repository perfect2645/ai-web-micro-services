/**
 * 对应C#端的ImageRecognitionStatus枚举
 * 需和服务端枚举值完全一致（大小写、值都要匹配）
 */
export enum ImageRecognitionStatus {
  // 示例值，替换为你C#端实际的枚举值（比如Prompt/Processing/Success/Failed等）
  Prompt = "Prompt",
  Processing = "Processing",
  Success = "Success",
  Failed = "Failed",
}

/**
 * 对应C#端的DoraemonItem record
 * 严格匹配必填/可选、类型，对齐IEntity/IEntityTiming接口的字段
 */
export interface DoraemonItem {
  /** 主键ID（C#端init赋值，TS中为必选） */
  id: string; // Guid → string

  /** 创建时间（C#端init赋值，必选） */
  createTime: string; // DateTime → ISO字符串

  /** 更新时间（可空） */
  updateTime?: string; // DateTime? → string | undefined

  /** 识别状态（必填，对应枚举） */
  status: ImageRecognitionStatus;

  /** 错误信息（可选，默认空字符串） */
  errorMessage?: string; // string? → string | undefined

  /** 输出图片ID（可空） */
  outputImageId?: string; // Guid? → string | undefined

  /** 输出图片URL（可空） */
  outputImageUrl?: string; // string? → string | undefined

  // ========== 业务字段 ==========
  /** 用户ID（必填，最大长度64） */
  userId: string; // [Required][MaxLength(64)]

  /** 输入图片ID（必填） */
  inputImageId: string; // [Required] Guid → string

  /** 输入图片URL（必填） */
  inputImageUrl: string; // [Required]

  /** 提示文本（可选，最大长度512） */
  promptText?: string; // [MaxLength(512)] string?
}

/**
 * 对应C#端的ITopicPayload接口（如果无属性，仅做类型标记）
 */
export interface ITopicPayload {
  topic: string;
}

/**
 * 对应C#端的DoraemonMessage record
 * 继承ITopicPayload，和服务端结构完全对齐
 */
export interface DoraemonMessage extends ITopicPayload {
  /** 消息主题（必填） */
  topic: string;

  /** 核心消息体（必填） */
  doraemonItem: DoraemonItem;

  /** 消息来源（可选） */
  source?: string; // string? → string | undefined
}
