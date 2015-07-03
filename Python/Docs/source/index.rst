QuickStart
==========

.. toctree::
    :maxdepth: 2
    :hidden:

    self
    examples
    api
    simulator


Writing python plugins for Else is trivial.  You can run Else plugins standalone and later package them for use in the Else app.




Minimum Requirements
--------------------
* The plugin has it's own directory, and there must be a module with the same name as the directory (e.g. /MyPlugin/MyPlugin.py).
* The plugin module provides a setup() method and a PLUGIN_NAME string variable.




Basic Plugin
------------

This plugin provides 1 command that responds to the keyword 'google' and requires arguments.

For example, the query "google Else Windows" will offer to open chrome.exe at the google search page for "Else Windows".

.. literalinclude:: ../../Lib/demo_simple_plugin/demo_simple_plugin.py


Whats next?
-----------

For more examples, see :doc:`examples`.

For API documentation, see :doc:`api`.

