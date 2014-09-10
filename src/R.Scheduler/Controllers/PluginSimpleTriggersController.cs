﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using log4net;
using R.Scheduler.Contracts.DataContracts;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class PluginSimpleTriggersController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly IPluginStore _pluginRepository;
        readonly ISchedulerCore _schedulerCore;

        public PluginSimpleTriggersController()
        {
            _pluginRepository = ObjectFactory.GetInstance<IPluginStore>();
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        [AcceptVerbs("POST")]
        [Route("api/plugins/simpleTriggers")]
        public void Post([FromBody]PluginSimpleTrigger model)
        {
            Logger.InfoFormat("Entered PluginSimpleTriggersController.Post(). PluginName = {0}", model.PluginName);

            var registeredPlugin = _pluginRepository.GetRegisteredPlugin(model.PluginName);

            if (null == registeredPlugin)
                throw new ArgumentException(string.Format("Error loading registered plugin {0}", model.PluginName));

            _schedulerCore.ScheduleSimpleTrigger(new SimpleTrigger
            {
                GroupName = model.PluginName,
                JobName = "Job_" + model.PluginName,
                RepeatCount = model.RepeatCount,
                RepeatInterval = model.RepeatInterval,
                StartDateTime = model.StartDateTime,
                TriggerName = model.TriggerName,
                DataMap = new Dictionary<string, object> { { "pluginPath", registeredPlugin.AssemblyPath } }
            });
        }

        [AcceptVerbs("DELETE")]
        [Route("api/plugins/simpleTriggers")]
        public void Delete(string pluginName, string triggerName)
        {
            Logger.InfoFormat("Entered PluginSimpleTriggersController.Delete(). pluginName = {0}. triggerName = {1}", pluginName, triggerName);

            _schedulerCore.DescheduleTrigger(pluginName, triggerName);
        }
    }
}