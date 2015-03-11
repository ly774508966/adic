﻿using UnityEngine;
using System;
using System.Collections.Generic;
using Adic.Binding;
using Adic.Container;
using Adic.Injection;

namespace Adic {
	/// <summary>
	/// Event caller container extension.
	/// 
	/// Intercepts bindings and resolutions to check whether objects
	/// should be added to receive update/dispose events.
	/// </summary>
	public class EventCallerContainerExtension : IContainerExtension {
		/// <summary>The disposable objects.</summary>
		public static List<IDisposable> disposable = new List<IDisposable>();
		/// <summary>The updateable objects.</summary>
		public static List<IUpdatable> updateable = new List<IUpdatable>();

		public GameObject eventCaller;

		/// <summary>
		/// Initializes a new instance of the <see cref="Adic.EventCallerContainerExtension"/> class.
		/// </summary>
		public EventCallerContainerExtension() {
			//Creates a new game object for UpdateableBehaviour.
			this.eventCaller = new GameObject();
			this.eventCaller.name = "EventCaller";
			this.eventCaller.AddComponent<EventCallerBehaviour>();
		}

		public void OnRegister(IInjectionContainer container) {
			//Adds the container to the disposable list.
			disposable.Add(container);

			//Checks whether a binding for the CommandDispatcher exists.
			if (container.ContainsBindingFor<CommandDispatcher>()) {
				var dispatcher = container.Resolve<CommandDispatcher>();
				disposable.Add(dispatcher);
			}

			container.afterAddBinding += this.OnAfterAddBinding;
			container.bindingResolution += this.OnBindingResolution;
		}
		
		public void OnUnregister(IInjectionContainer container) {
			container.afterAddBinding -= this.OnAfterAddBinding;
			container.bindingResolution -= this.OnBindingResolution;

			disposable.Clear();
			updateable.Clear();
			MonoBehaviour.Destroy(this.eventCaller);
		}

		/// <summary>
		/// handles the after add binding event.
		/// 
		/// Used to check whether singleton instances should be added to the updater.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="binding">Binding.</param>
		protected void OnAfterAddBinding(IBinder source, ref BindingInfo binding) {
			if (binding.instanceType == BindingInstance.Singleton) {				
				//Do not add commands.
				if (binding.value is ICommand) return;

				if (binding.value is IDisposable) {
					disposable.Add((IDisposable)binding.value);
				}
				if (binding.value is IUpdatable) {
					updateable.Add((IUpdatable)binding.value);
				}
			}
		}

		/// <summary>
		/// Handles the binding resolution event.
		/// 
		/// Used to check whether the resolved instance should be added to the updater.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="binding">Binding.</param>
		/// <param name="instance">Instance.</param>
		protected void OnBindingResolution(IInjector source, ref BindingInfo binding, ref object instance) {
			//Do not add commands.
			if (binding.instanceType == BindingInstance.Singleton || instance is ICommand) return;

			if (instance is IDisposable) {
				disposable.Add((IDisposable)instance);
			}
			if (instance is IUpdatable) {
				updateable.Add((IUpdatable)instance);
			}
		}
	}
}