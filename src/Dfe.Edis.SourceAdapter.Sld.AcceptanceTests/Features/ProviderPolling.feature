Feature: System polls for providers
  The system should check on a schedule, whether 
  
  Scenario: Provider data available
    Given A provider submits data for the first time
    When the system polls for providers
    Then the providers data should be published to Kafka