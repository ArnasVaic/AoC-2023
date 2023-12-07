# Advent of Code 2023

## Foreword

This year is different. Instead of trying to learn a language like last year with Haskell, this year is for perfecting a language that is already familiar - C#. Besides writing solutions for each day this project also embodies my attempt to write a framework for further Advent of Code events if I continue to write it in C#.

## How does it work ?

The stereotypical user of this framework is concerned with one thing and one thing only - solving the problem at hand.
Solution to each problem can be described in two steps:

1) Parse the input text to a suitable structure.

2) Write an algorithm to extract the answer from said structure.

For this reason the framework supplies the user with a minimal amount of methods to implement.
The example below illustrates how one would use this framework to write a solution.

First the ISolution