# Reflection
This document discusses design decisions, the integration of design patterns and insights gained from this exercise.

## Desgin Decisions
.NET 9 and Blazor were chosen given their simplicity and the ability to integrate multiple projects into one solution. Additionally, the creation of a Web API is a simple addition with the provided templates.

A blazor web app integrates a single page web app with a server backend, without having to code them seperately, sharing classes, functions. A single component was created for the the UI, the querying logic was put into a businesslogic class allowing for other components to theoretically access it's functionality. Furthermore, this allows for the Resilience Patterns to be implemented in a class where they are actually being used.

A Class Library Project was added, so that both the Blazor Web App and the Web API could use the same class, for easier serialization without having to add the class in both projects seperately. If in the future more connections would be needed, this is a way to share data contracts.

The Web API with the Asp .NET controller logic is easy to set up and allowed for smooth integration.

## Integration of Patterns
The patterns chosen were the Circuit Breaker, Retry and Timeout. I thought about which patterns provided a good connection and made sense to be applied onto a single request.

The circuit breaker allows for a general "cooldown" of the services, giving the service a break and time to fix issues and free up resources. But in order to make sure a single error doesn't instantly create a break, the retry pattern was used. As a way to check that the error was not a single anomaly. Only after a few failed request (including retries) it makes sense to give the service a break.

The timeout pattern just integrated generally, the time critically was given through the fact, that a user would probably continue to click the button creating a prompt until they find sth they like. If the service takes multiple seconds the user will not wait for suggestions, therefore it is an implicit requirement for the service to provide under the timeout time. However, in general the pattern makes sense to free up resources if the timing of the result is more important then having it.

## Insights gained
I had not previously implemented resilience patterns, however it is nice to be able to provide a website, where an error does not have to be instantly displayed, as retries can be run. Additionally, the patterns make sense if one is to host applications and services for example in a Kubernetes cluster or any other distributed system. 

The patterns per se are also relatively simple to implement, and preparing a generic function can provide easy access to the patterns in more use cases.