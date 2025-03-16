**New! Improved!**

Agreed, my original answer didn't quite nail it. Today I woke up fresh with a **_new plausible and provable theory of failure_** about what could give the appearance of a thread being blocked during the execution of a process that the OP states _"may import one million lines from a text file"_. 

The latest comment clarified:

> The "real problem" is that an application that runs a 10 minute method appears dead! I just want to post smething in the ui to let the user know it is still alive, and to wait for completion. 

As stated in the comment by Panagiotis Kanavos, on one hand this seems like a straightforward application for the `Progress` class. So let's "do everything right" in that respect and set up a minimal reproducible example that does a `ReadLineAsync()` one million times and attempts to update progress.

```

```