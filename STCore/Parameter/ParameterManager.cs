using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using STCore.Log;

namespace STCore.Parameter
{
    #region 配置实体类（按你的分类定义）
    /// <summary>
    /// 设备基本信息
    /// </summary>
    public class DeviceInfo
    {
        public string DeviceName { get; set; } = "STCore工控设备";
        public string DeviceModel { get; set; } = "ST-1000";
        public string SerialNumber { get; set; } = "SN20260605001";
        public string Manufacturer { get; set; } = "STCore";
        public DateTime ProductionDate { get; set; } = DateTime.Now;
        public string SoftwareVersion { get; set; } = "1.0.0";
    }

    /// <summary>
    /// 板卡参数
    /// </summary>
    public class BoardParam
    {
        public string CardType { get; set; } = "LeiSai";
        public int CardNo { get; set; } = 0;
        public int AxisCount { get; set; } = 4;
        public string IPAddress { get; set; } = "192.168.1.100";
        public int Port { get; set; } = 502;
        public int TimeoutMs { get; set; } = 3000;
    }

    /// <summary>
    /// 单轴参数
    /// </summary>
    public class AxisParam
    {
        public int AxisNo { get; set; }
        public string AxisName { get; set; }
        public double Speed { get; set; } = 100.0;
        public double HomeSpeed { get; set; } = 50.0;
        public double Acc { get; set; } = 500.0;
        public double Dec { get; set; } = 500.0;
        public double HomeOffset { get; set; } = 0.0;
        public double SoftLimitPositive { get; set; } = 1000.0;
        public double SoftLimitNegative { get; set; } = -1000.0;
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// IO配置
    /// </summary>
    public class IOConfig
    {
        public int IONo { get; set; }
        public string IOName { get; set; }
        public IOType Type { get; set; }
        public string Remark { get; set; }
        public bool IsInverted { get; set; } = false;
    }

    public enum IOType
    {
        Input,
        Output
    }

    /// <summary>
    /// 报警代码
    /// </summary>
    public class AlarmCode
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public AlarmLevel Level { get; set; }
        public string Solution { get; set; }
    }

    public enum AlarmLevel
    {
        Info,
        Warning,
        Error,
        Fatal
    }

    /// <summary>
    /// 系统设置
    /// </summary>
    public class SystemSettings
    {
        public string Language { get; set; } = "zh-CN";
        public int LogRetainDays { get; set; } = 30;
        public int AutoSaveIntervalMin { get; set; } = 60;
        public bool AutoStartOnBoot { get; set; } = false;
        public bool ShowDebugInfo { get; set; } = false;
        public bool EnablePassword { get; set; } = true;
    }

    /// <summary>
    /// 程序运行参数
    /// </summary>
    public class ProgramParam
    {
        public string ParamName { get; set; }
        public object Value { get; set; }
        public string Unit { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// 开关参数
    /// </summary>
    public class SwitchParam
    {
        public string SwitchName { get; set; }
        public bool IsEnabled { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// 补偿参数
    /// </summary>
    public class CompensationParam
    {
        public string CompName { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public string Remark { get; set; }
    }
    #endregion

    /// <summary>
    /// 多文件分类式参数管理器
    /// 支持XML/INI双格式，每个分类独立文件
    /// </summary>
    public class ParameterManager
    {
        #region 单例模式
        public static ParameterManager Instance { get; } = new ParameterManager();
        private ParameterManager()
        {
            InitializeDefaultValues();
        }
        #endregion

        #region 配置项
        /// <summary>
        /// 配置文件根目录
        /// </summary>
        public string ConfigRootPath { get; set; } = "Config";

        /// <summary>
        /// 配置文件格式（Xml/Ini）
        /// </summary>
        public ConfigFormat FileFormat { get; set; } = ConfigFormat.Xml;

        /// <summary>
        /// 自动备份数量
        /// </summary>
        public int BackupCount { get; set; } = 5;

        /// <summary>
        /// 是否自动保存
        /// </summary>
        public bool AutoSave { get; set; } = true;
        #endregion

        #region 配置数据（运行时内存中的数据）
        public DeviceInfo DeviceInfo { get; set; } = new DeviceInfo();
        public BoardParam BoardParam { get; set; } = new BoardParam();
        public List<AxisParam> AxisParams { get; set; } = new List<AxisParam>();
        public List<IOConfig> IOConfigs { get; set; } = new List<IOConfig>();
        public List<AlarmCode> AlarmCodes { get; set; } = new List<AlarmCode>();
        public SystemSettings SystemSettings { get; set; } = new SystemSettings();
        public List<ProgramParam> ProgramParams { get; set; } = new List<ProgramParam>();
        public List<SwitchParam> SwitchParams { get; set; } = new List<SwitchParam>();
        public List<CompensationParam> CompensationParams { get; set; } = new List<CompensationParam>();
        #endregion

        #region 私有字段
        private readonly object _lockObj = new object();
        private readonly XmlSerializerNamespaces _xmlNamespaces = new XmlSerializerNamespaces();
        #endregion

        #region 初始化
        private void InitializeDefaultValues()
        {
            _xmlNamespaces.Add("", "");

            // 初始化默认轴参数
            AxisParams.Add(new AxisParam { AxisNo = 0, AxisName = "X轴" });
            AxisParams.Add(new AxisParam { AxisNo = 1, AxisName = "Y轴" });
            AxisParams.Add(new AxisParam { AxisNo = 2, AxisName = "Z轴" });
            AxisParams.Add(new AxisParam { AxisNo = 3, AxisName = "R轴" });

            // 初始化默认IO配置
            IOConfigs.Add(new IOConfig { IONo = 0, IOName = "安全门", Type = IOType.Input });
            IOConfigs.Add(new IOConfig { IONo = 1, IOName = "急停", Type = IOType.Input });
            IOConfigs.Add(new IOConfig { IONo = 0, IOName = "报警灯红", Type = IOType.Output });

            // 初始化默认报警代码
            AlarmCodes.Add(new AlarmCode { Code = 1001, Message = "急停按下", Level = AlarmLevel.Fatal, Solution = "松开急停按钮" });
            AlarmCodes.Add(new AlarmCode { Code = 1002, Message = "安全门打开", Level = AlarmLevel.Error, Solution = "关闭安全门" });

            // 初始化默认程序参数
            ProgramParams.Add(new ProgramParam { ParamName = "夹紧延时", Value = 1000, Unit = "ms", Remark = "气缸夹紧等待时间" });
            ProgramParams.Add(new ProgramParam { ParamName = "真空延时", Value = 500, Unit = "ms", Remark = "真空建立等待时间" });

            // 初始化默认开关参数
            SwitchParams.Add(new SwitchParam { SwitchName = "自动回零", IsEnabled = true, Remark = "开机自动回零" });
            SwitchParams.Add(new SwitchParam { SwitchName = "报警蜂鸣", IsEnabled = true, Remark = "报警时蜂鸣器响" });

            // 初始化默认补偿参数
            CompensationParams.Add(new CompensationParam { CompName = "X轴机械补偿", Value = 0.0, Unit = "mm", Remark = "X轴机械误差补偿" });
            CompensationParams.Add(new CompensationParam { CompName = "Y轴机械补偿", Value = 0.0, Unit = "mm", Remark = "Y轴机械误差补偿" });
        }
        #endregion

        #region 核心加载保存方法


        /// <summary>
        /// 加载单个配置文件
        /// </summary>
        private T LoadConfig<T>(string fileName, T defaultValue)
        {
            string filePath = GetConfigFilePath(fileName);

            if (!File.Exists(filePath))
            {
                STLog.Warn($"配置文件{fileName}不存在，将使用默认值", LogCategory.System);
                SaveConfig(fileName, defaultValue);
                return defaultValue;
            }

            lock (_lockObj)
            {
                if (FileFormat == ConfigFormat.Xml)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        return (T)serializer.Deserialize(sr);
                    }
                }
                // TODO: 实现INI格式加载
            }

            STLog.Debug($"配置文件{fileName}加载成功", LogCategory.System);
            return defaultValue;
        }
        public bool LoadAll()
        {
            try
            {
                STLog.Info("开始加载所有配置文件...", LogCategory.System);

                if (!Directory.Exists(ConfigRootPath))
                {
                    Directory.CreateDirectory(ConfigRootPath);
                }

                DeviceInfo = LoadConfig("DeviceInfo", DeviceInfo);
                BoardParam = LoadConfig("BoardParams", BoardParam);
                AxisParams = LoadConfig("AxisParams", AxisParams);
                IOConfigs = LoadConfig("IOConfig", IOConfigs);
                AlarmCodes = LoadConfig("AlarmCodes", AlarmCodes);
                SystemSettings = LoadConfig("SystemSettings", SystemSettings);
                ProgramParams = LoadConfig("ProgramParams", ProgramParams);
                SwitchParams = LoadConfig("SwitchParams", SwitchParams);
                CompensationParams = LoadConfig("CompensationParams", CompensationParams);

                STLog.Info("所有配置文件加载成功", LogCategory.System);
                return true;
            }
            catch (Exception ex)
            {
                STLog.Error($"配置文件加载失败：{ex.Message}", ex, LogCategory.System);
                // 加载失败使用默认值
                InitializeDefaultValues();
                return false;
            }
        }
        /// <summary>
        /// 保存所有配置文件
        /// </summary>
        public bool SaveAll()
        {
            try
            {
                STLog.Info("开始保存所有配置文件...", LogCategory.System);

                // 自动备份
                BackupAll();

                // 按顺序保存所有配置
                SaveConfig("DeviceInfo", DeviceInfo);
                SaveConfig("BoardParams", BoardParam);
                SaveConfig("AxisParams", AxisParams);
                SaveConfig("IOConfig", IOConfigs);
                SaveConfig("AlarmCodes", AlarmCodes);
                SaveConfig("SystemSettings", SystemSettings);
                SaveConfig("ProgramParams", ProgramParams);
                SaveConfig("SwitchParams", SwitchParams);
                SaveConfig("CompensationParams", CompensationParams);

                STLog.Info("所有配置文件保存成功", LogCategory.System);
                return true;
            }
            catch (Exception ex)
            {
                STLog.Error($"配置文件保存失败：{ex.Message}", ex, LogCategory.System);
                return false;
            }
        }

        /// <summary>
        /// 保存单个配置文件
        /// </summary>
        private void SaveConfig<T>(string fileName, T data)
        {
            string filePath = GetConfigFilePath(fileName);

            lock (_lockObj)
            {
                if (FileFormat == ConfigFormat.Xml)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        serializer.Serialize(sw, data, _xmlNamespaces);
                    }
                }
                // TODO: 实现INI格式保存
            }

            STLog.Debug($"配置文件{fileName}保存成功", LogCategory.System);
        }
        #endregion

        #region 备份管理
        /// <summary>
        /// 备份所有配置文件
        /// </summary>
        public void BackupAll()
        {
            try
            {
                string backupDir = Path.Combine(ConfigRootPath, "Backup");
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                string backupTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupSubDir = Path.Combine(backupDir, backupTime);
                Directory.CreateDirectory(backupSubDir);

                // 复制所有配置文件到备份目录
                string[] configFiles = Directory.GetFiles(ConfigRootPath, $"*.{FileFormat.ToString().ToLower()}");
                foreach (string file in configFiles)
                {
                    string fileName = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(backupSubDir, fileName), true);
                }

                // 删除过期备份
                string[] backupDirs = Directory.GetDirectories(backupDir)
                    .OrderByDescending(d => d)
                    .ToArray();

                if (backupDirs.Length > BackupCount)
                {
                    for (int i = BackupCount; i < backupDirs.Length; i++)
                    {
                        Directory.Delete(backupDirs[i], true);
                    }
                }

                STLog.Info($"所有配置已备份到：{backupSubDir}", LogCategory.System);
            }
            catch (Exception ex)
            {
                STLog.Warn($"配置备份失败：{ex.Message}", LogCategory.System);
            }
        }

        /// <summary>
        /// 从指定备份恢复
        /// </summary>
        public bool RestoreFromBackup(string backupDirPath)
        {
            try
            {
                if (!Directory.Exists(backupDirPath))
                {
                    STLog.Error($"备份目录不存在：{backupDirPath}");
                    return false;
                }

                // 先备份当前配置
                BackupAll();

                // 复制备份文件到配置目录
                string[] backupFiles = Directory.GetFiles(backupDirPath);
                foreach (string file in backupFiles)
                {
                    string fileName = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(ConfigRootPath, fileName), true);
                }

                // 重新加载
                LoadAll();

                STLog.Info($"配置已从{backupDirPath}恢复", LogCategory.System);
                return true;
            }
            catch (Exception ex)
            {
                STLog.Error($"配置恢复失败：{ex.Message}", ex, LogCategory.System);
                return false;
            }
        }
        #endregion

        #region 便捷获取方法
        /// <summary>
        /// 根据轴号获取轴参数
        /// </summary>
        public AxisParam GetAxisParam(int axisNo)
        {
            return AxisParams.FirstOrDefault(a => a.AxisNo == axisNo);
        }

        /// <summary>
        /// 根据IO号和类型获取IO配置
        /// </summary>
        public IOConfig GetIOConfig(int ioNo, IOType type)
        {
            return IOConfigs.FirstOrDefault(io => io.IONo == ioNo && io.Type == type);
        }

        /// <summary>
        /// 根据报警代码获取报警信息
        /// </summary>
        public AlarmCode GetAlarmCode(int code)
        {
            return AlarmCodes.FirstOrDefault(a => a.Code == code);
        }

        /// <summary>
        /// 根据名称获取程序参数
        /// </summary>
        public T GetProgramParam<T>(string paramName, T defaultValue = default)
        {
            var param = ProgramParams.FirstOrDefault(p => p.ParamName == paramName);
            if (param == null) return defaultValue;

            try
            {
                return (T)Convert.ChangeType(param.Value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 根据名称获取开关参数
        /// </summary>
        public bool GetSwitchParam(string switchName, bool defaultValue = false)
        {
            var sw = SwitchParams.FirstOrDefault(s => s.SwitchName == switchName);
            return sw?.IsEnabled ?? defaultValue;
        }

        /// <summary>
        /// 根据名称获取补偿参数
        /// </summary>
        public double GetCompensationParam(string compName, double defaultValue = 0.0)
        {
            var comp = CompensationParams.FirstOrDefault(c => c.CompName == compName);
            return comp?.Value ?? defaultValue;
        }
        #endregion

        #region 便捷设置方法
        /// <summary>
        /// 设置程序参数
        /// </summary>
        public bool SetProgramParam(string paramName, object value)
        {
            var param = ProgramParams.FirstOrDefault(p => p.ParamName == paramName);
            if (param == null)
            {
                STLog.Warn($"尝试设置不存在的程序参数：{paramName}", LogCategory.System);
                return false;
            }

            param.Value = value;
            if (AutoSave) SaveAll();
            return true;
        }

        /// <summary>
        /// 设置开关参数
        /// </summary>
        public bool SetSwitchParam(string switchName, bool isEnabled)
        {
            var sw = SwitchParams.FirstOrDefault(s => s.SwitchName == switchName);
            if (sw == null)
            {
                STLog.Warn($"尝试设置不存在的开关参数：{switchName}", LogCategory.System);
                return false;
            }

            sw.IsEnabled = isEnabled;
            if (AutoSave) SaveAll();
            return true;
        }

        /// <summary>
        /// 设置补偿参数
        /// </summary>
        public bool SetCompensationParam(string compName, double value)
        {
            var comp = CompensationParams.FirstOrDefault(c => c.CompName == compName);
            if (comp == null)
            {
                STLog.Warn($"尝试设置不存在的补偿参数：{compName}", LogCategory.System);
                return false;
            }

            comp.Value = value;
            if (AutoSave) SaveAll();
            return true;
        }
        #endregion

        #region 辅助方法
        private string GetConfigFilePath(string fileName)
        {
            return Path.Combine(ConfigRootPath, $"{fileName}.{FileFormat.ToString().ToLower()}");
        }

        /// <summary>
        /// 恢复所有配置到默认值
        /// </summary>
        public void RestoreAllDefaults()
        {
            InitializeDefaultValues();
            SaveAll();
            STLog.Warn("所有配置已恢复到默认值", LogCategory.System);
        }
        #endregion
    }

    public enum ConfigFormat
    {
        Xml,
        Ini
    }
}
