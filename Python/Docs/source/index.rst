QuickStart
==========

.. toctree::
    :maxdepth: 2
    :hidden:

    self
    examples
    api
    simulator


Writing python plugins for Else is trivial.(?????)




Requirements
------------
* The plugin has it's own directory, and there must be a module with the same name as the directory (e.g. /MyPlugin/MyPlugin.py).
* The plugin module provides a PLUGIN_NAME string variable.
* The plugin module provides a setup() method, which registers any number of :class:`Else.Command` or :class:`Else.ResultProvider` instances.

Brief Description
-----------------

A Plugin has its own directory, and there must be a module that matches the name of the directory (e.g. /MyPlugin/MyPlugin.py).

There are 2 types of objects that can be registered by a plugin to interact with Else, in the plugins setup() method.


:class:`Else.Command` is an easy way to setup a command.  For example, you could respond to the query 'reddit' and if selected you could start a browser at reddit.com.  Or if the query was 'reddit funny', you could start a browser at reddit.com/r/funny.

:class:`Else.ResultProvider` is more flexible.  For example you could provide a math calculator that parses the current query (e.g. "4+4+8*15), and provides a result, which upon selection copies the result to the clipboard.  Or you could listen to the keyword "reddit" and when selected, provide results for the top 10 reddit posts.







Basic Plugin
------------

This plugin provides 1 command that responds to the keyword 'google' and requires arguments.

For example, the query "google Else Windows" will offer to open chrome.exe at the google search page for "Else Windows".

.. literalinclude:: ../../Lib/demo_simple_plugin/demo_simple_plugin.py


Whats next?
-----------

For more examples, see :doc:`examples`.

For API documentation, see :doc:`api`.

