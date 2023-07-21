let fib :: Int n -> Int {
	let n1 = n - 1;
	let n2 = n - 2;

	if n <= 2 then 1 else ((fib n1) + (fib n2))
}

let main = fib 5;