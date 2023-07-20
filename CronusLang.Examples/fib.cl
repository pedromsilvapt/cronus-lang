let fib :: Int n -> Int {
	let n1 = fib (n - 1);
	let n2 = fib (n - 2);

	if n <= 2 then 1 else n1 + n2
}

let main = fib 5;