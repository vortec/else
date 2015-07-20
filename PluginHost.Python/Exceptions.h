#pragma once
using namespace System;

namespace Else {
	namespace PythonPluginLoader {
		/// <summary>
		/// An exception occurred in the python environment.
		/// </summary>
		ref class PythonException : public Exception
		{
		public:
			PythonException(String^ msg) : Exception(msg) {}
		};
	}
}