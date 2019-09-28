package com.jetbrains.fortea.configuration.execution.impl

import com.intellij.execution.*
import com.intellij.execution.executors.DefaultRunExecutor
import com.intellij.execution.process.NopProcessHandler
import com.intellij.execution.runners.ExecutionEnvironmentBuilder
import com.intellij.openapi.project.Project
import com.intellij.util.concurrency.Semaphore
import com.jetbrains.rdclient.util.idea.pumpMessages
import com.jetbrains.rider.projectView.ProjectModelViewHost

class T4SynchronousRunConfigurationExecutor(
  project: Project,
  host: ProjectModelViewHost
) : T4RunConfigurationExecutorBase(project, host) {
  private val executor: Executor = DefaultRunExecutor.getRunExecutorInstance()

  override fun executeConfiguration(configuration: RunnerAndConfigurationSettings) {
    val builder: ExecutionEnvironmentBuilder
    try {
      builder = ExecutionEnvironmentBuilder.create(executor, configuration)
    } catch (e: ExecutionException) {
      return
    }

    val finished = Semaphore()
    finished.down()

    val environment = builder.contentToReuse(null).dataContext(null).activeTarget().build()
    ProgramRunnerUtil.executeConfigurationAsync(
      environment,
      true,
      true
    ) {
      val listener = project.messageBus.syncPublisher(ExecutionManager.EXECUTION_TOPIC)
      listener.processStarted(executor.id, environment, NopProcessHandler())
      finished.up()
    }
    waitPumping(finished)
  }

  private tailrec fun waitPumping(finished: Semaphore) {
    val acquired = finished.waitFor(10)
    if (acquired) return
    pumpMessages(10)
    waitPumping(finished)
  }
}
